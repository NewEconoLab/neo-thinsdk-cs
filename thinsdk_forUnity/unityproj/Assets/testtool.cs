using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class testtool : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnGUI()
    {
        if (GUILayout.Button("test0"))
        {
            var bi = new BigMath.Numerics.BigInteger(256);
            var strline = "";
            foreach (var b in bi.ToByteArray())
            {
                strline += b.ToString("X02");
            }
            Debug.Log(bi + "=" + strline);

            bi = new BigMath.Numerics.BigInteger(-11233);
            strline = "";
            foreach (var b in bi.ToByteArray())
            {
                strline += b.ToString("X02");
            }
            Debug.Log(bi + "=" + strline);


            bi = new BigMath.Numerics.BigInteger(new byte[] { 0x00, 0x02 });
            strline = "";
            foreach (var b in bi.ToByteArray())
            {
                strline += b.ToString("X02");
            }
            Debug.Log(bi + "=" + strline);

            bi = new BigMath.Numerics.BigInteger(new byte[] { 0x01, 0xff });
            strline = "";
            foreach (var b in bi.ToByteArray())
            {
                strline += b.ToString("X02");
            }
            Debug.Log(bi + "=" + strline);

            bi = new BigMath.Numerics.BigInteger(new byte[] { 0x01, 0xff, 0x00 });
            strline = "";
            foreach (var b in bi.ToByteArray())
            {
                strline += b.ToString("X02");
            }
            Debug.Log(bi + "=" + strline);
        }
        if (GUILayout.Button("test1 CheckAddress"))
        {
            var addr = "ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj";
            var uint8 = ThinNeo.Helper.GetPublicKeyHashFromAddress(addr);
            //.GetPublicKeyScriptHash_FromAddress(addr);
            var hexstr = uint8.ToString();
            Debug.Log("addr=" + addr);
            Debug.Log("hex=" + hexstr);
        }
        if (GUILayout.Button("test2 Hash2Address"))
        {
            var hexstr = "0x0b193415c6f098b02e81a3b14d0e3b08e9c3f79a";
            //var hashrev = ThinNeo.Helper.HexString2Bytes(hexstr);
            var hash = new ThinNeo.Hash160(hexstr);
            var addr = ThinNeo.Helper.GetAddressFromScriptHash(hash);
            Debug.Log("hex=" + hexstr);
            Debug.Log("addr=" + addr);
        }
        if (GUILayout.Button("test3 Test_Pubkey2Address"))
        {
            var pubkey = "02bf055764de0320c8221920d856d3d9b93dfc1dcbc759a560fd42553aa025ba5c";
            var bytes = ThinNeo.Helper.HexString2Bytes(pubkey);
            var addr = ThinNeo.Helper.GetAddressFromPublicKey(bytes);
            Debug.Log("pubkey=" + pubkey);
            Debug.Log("addr=" + addr);
        }
        if (GUILayout.Button("test4 WifDecode"))
        {
            var wif = "L2CmHCqgeNHL1i9XFhTLzUXsdr5LGjag4d56YY98FqEi4j5d83Mv";
            var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var addr = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            Debug.Log("wif=" + wif);
            Debug.Log("prikey=" + ThinNeo.Helper.Bytes2HexString(prikey));
            Debug.Log("pubkey=" + ThinNeo.Helper.Bytes2HexString(pubkey));
            Debug.Log("addr=" + addr);
        }
        if (GUILayout.Button("test5 Sign&Vertify"))
        {
            var wif = "L2CmHCqgeNHL1i9XFhTLzUXsdr5LGjag4d56YY98FqEi4j5d83Mv";
            var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var addr = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            var signdata = "010203ff1122abcd";
            var message = ThinNeo.Helper.HexString2Bytes(signdata);
            var data = ThinNeo.Helper.Sign(message, prikey);
            Debug.Log("wif=" + wif);
            Debug.Log("addr=" + addr);
            Debug.Log("sign=" + ThinNeo.Helper.Bytes2HexString(data));

            var b = ThinNeo.Helper.VerifySignature(message, data, pubkey);
            Debug.Log("verify=" + b);
        }
        if (GUILayout.Button("test6 Nep2->Prikey"))
        {
            //這個需要把scrypt換掉
            var nep2 = "6PYT8kA51ffcAv3bJzbfcT6Uuc32QS5wHEjneRdkPYFxZSrirVHRPEpVwN";
            var n = 16384;
            var r = 8;
            var p = 8;
            var prikey = ThinNeo.Helper.GetPrivateKeyFromNEP2(nep2, "1", n, r, p);
            Debug.Log("result=" + ThinNeo.Helper.Bytes2HexString(prikey));
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            Debug.Log("address=" + address);


        }
    }
}
