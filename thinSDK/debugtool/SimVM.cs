using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinNeo.Debug
{
    public class State
    {
        public int StateID
        {
            get;
            private set;
        }
        public void SetId(int id)
        {
            this.StateID = id;
        }
        public VM.RandomAccessStack<string> ExeStack = new VM.RandomAccessStack<string>();
        public VM.RandomAccessStack<SmartContract.Debug.StackItem> CalcStack = new VM.RandomAccessStack<SmartContract.Debug.StackItem>();
        public VM.RandomAccessStack<SmartContract.Debug.StackItem> AltStack = new VM.RandomAccessStack<SmartContract.Debug.StackItem>();
        public void PushExe(string hash)
        {
            ExeStack.Push(hash);
            StateID++;
        }
        public void PopExe()
        {
            ExeStack.Pop();
            StateID++;
        }
        public bool CalcCalcStack(VM.OpCode op)
        {
            if (op == VM.OpCode.TOALTSTACK)
            {
                var p = CalcStack.Pop();
                AltStack.Push(p);
                StateID++;
                return true;
            }
            if (op == VM.OpCode.FROMALTSTACK)
            {
                var p = AltStack.Pop();
                CalcStack.Push(p);
                StateID++;
                return true;
            }
            if (op == VM.OpCode.XSWAP)
            {
                var item = CalcStack.Pop();
                var xn = CalcStack.Peek(item.AsInt());
                var swapv = CalcStack.Peek(0);
                CalcStack.Set(item.AsInt(), swapv);
                CalcStack.Set(0, xn);
                StateID++;
                return true;
            }
            if (op == VM.OpCode.SWAP)
            {
                var v1 = CalcStack.Pop();
                var v2 = CalcStack.Pop();
                CalcStack.Push(v1);
                CalcStack.Push(v2);
                StateID++;
                return true;
            }
            //if (op == VM.OpCode.ROLL)
            //{
            //    int n = (int)CalcStack.Pop().AsInt();
            //    CalcStack.Push(CalcStack.Remove(n));
            //    return true;
            //}
            return false;
        }
        public void CalcCalcStack(SmartContract.Debug.Op stackop, SmartContract.Debug.StackItem item)
        {
            if (stackop.type == SmartContract.Debug.OpType.Push)
            {
                if (item == null)
                    throw new Exception(stackop.type + "can not pass null");
                CalcStack.Push(item);
            }
            else if (stackop.type == SmartContract.Debug.OpType.Insert)
            {
                if (item == null)
                    throw new Exception(stackop.type + "can not pass null");
                CalcStack.Insert(stackop.ind, item);
            }
            else if (stackop.type == SmartContract.Debug.OpType.Clear)
            {
                CalcStack.Clear();
            }
            else if (stackop.type == SmartContract.Debug.OpType.Set)
            {
                if (item == null)
                    throw new Exception(stackop.type + "can not pass null");
                CalcStack.Set(stackop.ind, item);
            }
            else if (stackop.type == SmartContract.Debug.OpType.Pop)
            {
                CalcStack.Pop();
            }
            else if (stackop.type == SmartContract.Debug.OpType.Peek)
            {
                //CalcStack.Peek(stackop.ind);
            }
            else if (stackop.type == SmartContract.Debug.OpType.Remove)
            {
                CalcStack.Remove(stackop.ind);
            }
            if (stackop.type != SmartContract.Debug.OpType.Peek)//peek 不造成状态变化
                StateID++;
        }
        public void DoSysCall()
        {

        }
        public State Clone()
        {
            State state = new State();
            state.StateID = this.StateID;
            foreach (var s in ExeStack)
            {
                state.ExeStack.Push(s);
            }
            foreach (var s in CalcStack)
            {
                if (s == null)
                    state.CalcStack.Push(null);
                else
                    state.CalcStack.Push(s.Clone());
            }
            foreach (var s in AltStack)
            {
                state.AltStack.Push(s.Clone());
            }
            return state;
        }
    }
    public class CareItem
    {
        public CareItem(string name, State state)
        {
            this.name = name;
            if (name == "Neo.Runtime.CheckWitness" ||
               name == "Neo.Runtime.Notify")
            {
                this.item = state.CalcStack.Peek(0).Clone();
                //this.item = item.Conv2String();
            }
            else if (name == "Neo.Runtime.Log")
            {
                var item = state.CalcStack.Peek(0);
                this.item = new SmartContract.Debug.StackItem();
                this.item.type = "String";
                if (item.type == "String")
                {
                    this.item.strvalue = item.strvalue;
                }
                else if (item.type == "ByteArray")
                {
                    var bt = Debug.DebugTool.HexString2Bytes(item.strvalue);
                    this.item.strvalue = System.Text.Encoding.ASCII.GetString(bt);
                }
                else
                {
                    throw new Exception("can't conver this.");
                }
            }
            else if (name == "Neo.Storage.Put")
            {
                var item1 = state.CalcStack.Peek(0);
                var item2 = state.CalcStack.Peek(1);
                var item3 = state.CalcStack.Peek(2);
                this.item = new SmartContract.Debug.StackItem();
                this.item.type = "Array";
                this.item.subItems = new List<SmartContract.Debug.StackItem>();
                this.item.subItems.Add(item1.Clone());
                this.item.subItems.Add(item2.Clone());
                this.item.subItems.Add(item3.Clone());
            }
            else
            {

            }

        }
        public string name;
        public SmartContract.Debug.StackItem item;
        public override string ToString()
        {
            return name + "(" + item?.ToString() + ")";
        }
    }
    //模拟虚拟机
    public class SimVM
    {
        public void Execute(SmartContract.Debug.FullLog FullLog)
        {
            State runstate = new State();
            runstate.SetId(0);

            stateClone = new Dictionary<int, State>();
            mapState = new Dictionary<SmartContract.Debug.LogOp, int>();
            careinfo = new List<CareItem>();

            regenScript = new SmartContract.Debug.LogScript(FullLog.script.hash);
            lastScript = regenScript;

            ExecuteScript(runstate, FullLog.script);
        }
        SmartContract.Debug.LogScript lastScript = null;
        public SmartContract.Debug.LogScript regenScript;
        public Dictionary<int, State> stateClone;
        public Dictionary<SmartContract.Debug.LogOp, int> mapState;
        public List<CareItem> careinfo;
        //加入outputscript，做一個補救，以前導出的log樹層次可能是錯的
        void ExecuteScript(State runstate, SmartContract.Debug.LogScript script)
        {
            try
            {
                runstate.PushExe(script.hash);
                foreach (var op in script.ops)
                {
                    var _nop = op.Clone();
                    lastScript.ops.Add(_nop);
                    try
                    {
                        if (op.op == VM.OpCode.APPCALL)//不造成栈影响，由目标script影响
                        {
                            var _script = op.subScript;
                            var outscript = new SmartContract.Debug.LogScript(op.subScript.hash);
                            outscript.parent = lastScript;
                            _nop.subScript = outscript;
                            lastScript = outscript;
                            //有可能造成影响
                            if (op.stack != null)
                            {

                                for (var i = 0; i < op.stack.Length; i++)
                                {
                                    if (i == op.stack.Length - 1)
                                    {
                                        runstate.CalcCalcStack(op.stack[i], op.opresult);
                                    }
                                    else
                                    {
                                        runstate.CalcCalcStack(op.stack[i], null);
                                    }
                                }
                            }
                            runstate.PushExe(script.hash);
                            if (stateClone.ContainsKey(runstate.StateID) == false)
                            {
                                stateClone[runstate.StateID] = (Debug.State)runstate.Clone();
                            }
                            ExecuteScript(runstate, _script);
                            mapState[_nop] = runstate.StateID;
                        }
                        else if (op.op == VM.OpCode.CALL)//造成栈影响 就是个jmp
                        {
                            var _lastScript = new SmartContract.Debug.LogScript(lastScript.hash);
                            _lastScript.parent = lastScript;
                            _nop.subScript = _lastScript;

                            lastScript = _lastScript;
                            runstate.PushExe(lastScript.hash);
                            mapState[_nop] = runstate.StateID;

                            //runstate.callcount++;
                        }
                        else if (op.op == VM.OpCode.RET)
                        {
                            runstate.PopExe();

                            //mapState[op] = runstate.StateID;
                            //if (runstate.callcount > 0)
                            //{
                            //    runstate.callcount--;
                            //}
                            //if (runstate.callcount == 0)
                            {
                                lastScript = lastScript.parent;
                            }
                            if (stateClone.ContainsKey(runstate.StateID) == false)
                            {
                                stateClone[runstate.StateID] = (Debug.State)runstate.Clone();
                            }
                            mapState[_nop] = runstate.StateID;
                        }
                        else
                        {
                            if (op.op == VM.OpCode.SYSCALL)//syscall比较独特，有些syscall 可以产生独立的log
                            {
                                var name = System.Text.Encoding.ASCII.GetString(op.param);
                                careinfo.Add(new CareItem(name, runstate));
                                //runstate.DoSysCall(op.op);
                            }
                            if (runstate.CalcCalcStack(op.op) == false)
                            {
                                if (op.stack != null)
                                {

                                    for (var i = 0; i < op.stack.Length; i++)
                                    {
                                        if (i == op.stack.Length - 1)
                                        {
                                            runstate.CalcCalcStack(op.stack[i], op.opresult);
                                        }
                                        else
                                        {
                                            runstate.CalcCalcStack(op.stack[i], null);
                                        }
                                    }
                                }
                            }
                            if (stateClone.ContainsKey(runstate.StateID) == false)
                            {
                                stateClone[runstate.StateID] = (Debug.State)runstate.Clone();
                            }
                            mapState[_nop] = runstate.StateID;
                        }
                    }
                    catch (Exception err1)
                    {
                        _nop.error = true;
                    }
                }
            }
            catch (Exception err)
            {
                throw new Exception("error in:" + err.Message);
            }
        }
    }
}
