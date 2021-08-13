﻿

#define RSA_VERIFY_SUPPORT
//#define ECC_VERIFY_SUPPORT
//#define RSA_PSS_VERIFY_SUPPORT

#if RSA_PSS_VERIFY_SUPPORT
    // there is currently a bug in MS implementation which prevent proper use of RSA PSS    
    //#define USE_MICROSOFT_RSA_CNG
    #define USE_BOUNCYCASTLE
#endif

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;


#if USE_BOUNCYCASTLE
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto;
#endif



namespace ePassport
{
    public static class CryptoUtils
    {
        private static int GetHashAlgoOutputSizeFromOid(string algorithmOid)
        {
            KnownOids algorithmOidEnum = Oids.ParseKnown(algorithmOid);

            switch (algorithmOidEnum)
            {
                case KnownOids.sha1:
                    return (160 / 8);

                case KnownOids.sha256:
                    return (256 / 8);                    

                case KnownOids.sha384:
                    return (384 / 8);                    

                case KnownOids.sha512:
                    return (512 / 8);

                default:
                    throw new NotImplementedException("hash algorithm : " + algorithmOidEnum + "(" + algorithmOid + ") not yet implemented");
            }
        }

#if USE_BOUNCYCASTLE
        private static IDigest GetBouncyCastleHashAlgoInstanceFromOid(string algorithmOid)
        {
            GeneralDigest digestAlgo = new Sha1Digest();

            KnownOids algorithmOidEnum = Oids.ParseKnown(algorithmOid);            

            switch (algorithmOidEnum)
            {
                case KnownOids.sha1:
                    return new Sha1Digest();

                case KnownOids.sha256:
                    return new Sha256Digest();

                case KnownOids.sha384:
                    return new Sha384Digest();

                case KnownOids.sha512:
                    return new Sha512Digest();

                default:
                    throw new NotImplementedException("hash algorithm : " + algorithmOidEnum + "(" + algorithmOid + ") not yet implemented");
            }
        }
#endif

        private static System.Security.Cryptography.HashAlgorithm GetMicrosoftHashAlgoInstanceFromOid(string algorithmOid)
        {
            KnownOids algorithmOidEnum = Oids.ParseKnown(algorithmOid);

            switch (algorithmOidEnum)
            {
                case KnownOids.sha1:
                    return SHA1.Create();                    

                case KnownOids.sha256:
                    return SHA256.Create();

                case KnownOids.sha384:
                    return SHA384.Create();                    

                case KnownOids.sha512:
                    return SHA512.Create();                    

                default:
                    throw new NotImplementedException("hash algorithm : " + algorithmOidEnum + "(" + algorithmOid + ") not yet implemented");
            }
        }

        public static byte[] ComputeHash(string algorithmOid, byte[] data)
        {
            System.Security.Cryptography.HashAlgorithm hashAlgo = null;
            try
            {
                hashAlgo = GetMicrosoftHashAlgoInstanceFromOid(algorithmOid);
                return hashAlgo.ComputeHash(data);
            }
            finally
            {
                if (hashAlgo != null)
                {
                    hashAlgo.Dispose();
                }
            }            
        }

#if ECC_VERIFY_SUPPORT
        private static System.Security.Cryptography.ECPoint ByteArrayToCryptoECPoint(byte[] data)
        {            
            List<byte> dataList = new List<byte>(data);            
            if (dataList[0] == 0x04)
            {
                // uncompressed
                dataList.RemoveAt(0);                
            }

            int componentLen = dataList.Count / 2;

            System.Security.Cryptography.ECPoint ecPoint = new System.Security.Cryptography.ECPoint();
            ecPoint.X = dataList.GetRange(0, componentLen).ToArray();
            dataList.RemoveRange(0, componentLen);
            ecPoint.Y = dataList.GetRange(0, componentLen).ToArray();

            return ecPoint;
        }        
#endif

        public static bool VerifyDigestSignature(SubjectPublicKeyInfo subjectPublicKeyInfo, byte[] digestToVerify, string digestAlgorithmOid, byte[] signatureToVerify)
        {
            bool result = false;

            KnownOids algorithmOidEnum = Oids.ParseKnown(subjectPublicKeyInfo.Algorithm.Algorithm.Value);            

            switch (algorithmOidEnum)
            {
#if RSA_VERIFY_SUPPORT
                case KnownOids.rsaEncryption:
                    {
                        RSAPublicKey rsaPublicKey = Utils.DerDecode<RSAPublicKey>(subjectPublicKeyInfo.SubjectPublicKey.Value);

                        byte[] modulus = rsaPublicKey.Modulus.ToBigEndianByteArray();
                        byte[] exponent = rsaPublicKey.PublicExponent.ToBigEndianByteArray();

                        //Create a new instance of RSACryptoServiceProvider.
                        using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(modulus.Length * 8))
                        {
                            //Create a new instance of RSAParameters.
                            RSAParameters RSAKeyInfo = new RSAParameters();

                            //Set RSAKeyInfo to the public key values.                             
                            RSAKeyInfo.Modulus = modulus;
                            RSAKeyInfo.Exponent = exponent;

                            //Import key parameters into RSA.
                            RSA.ImportParameters(RSAKeyInfo);

                            if (digestToVerify.Length == GetHashAlgoOutputSizeFromOid(digestAlgorithmOid))
                            {
                                result = RSA.VerifyHash(digestToVerify, digestAlgorithmOid, signatureToVerify);
                            }
                        }
                    }
                    return result;
#endif

#if RSA_PSS_VERIFY_SUPPORT
                case KnownOids.rsassa_pss:
                    {
                        RSAPublicKey rsaPublicKey = Utils.DerDecode<RSAPublicKey>(subjectPublicKeyInfo.SubjectPublicKey.Value);

#if USE_BOUNCYCASTLE
                        int saltLength = 20;
                        IDigest digestAlgo = new Sha1Digest();                        

                        if (subjectPublicKeyInfo.Algorithm.isParametersPresent()) {
                            RSASSA_PSS_params rsassa_pss_params = Utils.DerDecode<RSASSA_PSS_params>(subjectPublicKeyInfo.Algorithm.Parameters);
                            digestAlgo = GetBouncyCastleHashAlgoInstanceFromOid(rsassa_pss_params.HashAlgorithm.Value.Algorithm.Value);
                            saltLength = (int)rsassa_pss_params.SaltLength;
                        }

                        RsaKeyParameters publickey = new RsaKeyParameters(
                            false, 
                            rsaPublicKey.Modulus.ToBouncyCastleBigInteger(),
                            rsaPublicKey.PublicExponent.ToBouncyCastleBigInteger()
                            );

                        PssSigner eng = new PssSigner(new RsaEngine(), digestAlgo, saltLength); //create new pss

                        eng.Init(false, publickey); //initiate this one

                        eng.BlockUpdate(digestToVerify, 0, digestToVerify.Length);

                        result = eng.VerifySignature(signatureToVerify);                    
#endif                        

#if USE_MICROSOFT_RSA_CNG
                        byte[] modulus = rsaPublicKey.Modulus.ToBigEndianByteArray();
                        byte[] exponent = rsaPublicKey.PublicExponent.ToBigEndianByteArray();

                        //Create a new instance of RSACryptoServiceProvider.
                        using (RSACng rsa = new RSACng(modulus.Length * 8))
                        {
                            //Create a new instance of RSAParameters.
                            RSAParameters RSAKeyInfo = new RSAParameters();

                            //Set RSAKeyInfo to the public key values.                             
                            RSAKeyInfo.Modulus = modulus;
                            RSAKeyInfo.Exponent = exponent;

                            //Import key parameters into RSA.
                            rsa.ImportParameters(RSAKeyInfo);

                            HashAlgorithmName hashAlgorithmName = HashAlgorithmName.SHA256;
                            RSASignaturePadding rsaSignaturePadding = RSASignaturePadding.Pss;

                            result = rsa.VerifyHash(digestToVerify, signatureToVerify, hashAlgorithmName, rsaSignaturePadding);                            
                        }
#endif

                    }
                    return result;
#endif

#if ECC_VERIFY_SUPPORT
                case KnownOids.ecPublicKey:
                    {
                        System.Security.Cryptography.ECParameters ecParameters = new System.Security.Cryptography.ECParameters();
                        ecParameters.Curve = new ECCurve();                        

                        ePassport.ECParameters decodedEcParameters = Utils.DerDecode<ePassport.ECParameters>(subjectPublicKeyInfo.Algorithm.Parameters);

                        if (decodedEcParameters.Version.Value == 1)
                        {
                            KnownOids fieldTypeOidEnum = Oids.ParseKnown(decodedEcParameters.FieldID.FieldType.Value);

                            switch (fieldTypeOidEnum)
                            {
                                case KnownOids.prime_field:
                                    {
                                        BigInteger primeBi = Utils.DerDecode<Prime_p>(decodedEcParameters.FieldID.Parameters).Value;
                                        ecParameters.Curve.Prime = primeBi.ToBigEndianByteArray();
                                        ecParameters.Curve.CurveType = ECCurve.ECCurveType.PrimeShortWeierstrass;
                                    }
                                    break;

                                case KnownOids.characteristic_two_field:
                                    throw new NotImplementedException("ECC field type : " + fieldTypeOidEnum + "(" + decodedEcParameters.FieldID.FieldType.Value + ") not yet implemented");

                                default:
                                    throw new NotImplementedException("ECC field type : " + fieldTypeOidEnum + "(" + decodedEcParameters.FieldID.FieldType.Value + ") not yet implemented");                                    
                            }

                            ecParameters.Curve.A = decodedEcParameters.Curve.A.Value;
                            ecParameters.Curve.B = decodedEcParameters.Curve.B.Value;
                            
                            if (decodedEcParameters.Curve.isSeedPresent())
                            {
                                ecParameters.Curve.Seed = decodedEcParameters.Curve.Seed.Value;                                
                            }

                            ecParameters.Curve.G = ByteArrayToCryptoECPoint(decodedEcParameters.Base.Value);
                            ecParameters.Curve.Order = decodedEcParameters.Order.ToBigEndianByteArray();

                            if (decodedEcParameters.isCofactorPresent())
                            {
                                ecParameters.Curve.Cofactor = decodedEcParameters.Cofactor.ToBigEndianByteArray();
                            }

                            ecParameters.Q = ByteArrayToCryptoECPoint(subjectPublicKeyInfo.SubjectPublicKey.Value);

                            ECDsa ecDsa = ECDsa.Create(ecParameters);                            

                            result = ecDsa.VerifyHash(digestToVerify, signatureToVerify);                            
                        }
                        else
                        {
                            throw new NotSupportedException("Only version1 of ECParameter is expected atm");
                        }
                    }
                    return result;
#endif

                        default:
                    throw new NotImplementedException("Signature Algorithm : " + algorithmOidEnum + "(" + subjectPublicKeyInfo.Algorithm.Algorithm.Value + ") not yet implemented");
            }
        }

        public static bool VerifySignedData(SignedData signedData)
        {
            Certificate certificate = null;
            return VerifySignedData(signedData, out certificate);
        }

        public static bool VerifySignedData(SignedData signedData, out Certificate certificate)
        {
            certificate = null;

            foreach (SignerInfo signerInfo in signedData.SignerInfos.Value)
            {
                byte[] digestToVerify = null;
                byte[] signature = null;
                
                string digestAlgorithmOid = signerInfo.DigestAlgorithm.Value.Algorithm.Value;                

                string signatureAlgorithmOid = signerInfo.SignatureAlgorithm.Value.Algorithm.Value;

                KnownOids signatureOidEnum = Oids.ParseKnown(signatureAlgorithmOid);
                
                switch (signatureOidEnum)
                {
                    case KnownOids.ecdsa_with_sha1:
                    case KnownOids.ecdsa_with_sha256:
                    case KnownOids.ecdsa_with_sha384:
                    case KnownOids.ecdsa_with_sha512:
                        digestToVerify = ComputeHash(digestAlgorithmOid, signedData.EncapContentInfo.EContent);
                        // in case of ecdsa, the signature is a sequence of r and s that needs to be concatenated
                        ECDSA_Sig_Value ecdsaSignature = Utils.DerDecode<ECDSA_Sig_Value>(signerInfo.Signature.Value);
                        List<byte> concatenated = new List<byte>();
                        concatenated.AddRange(ecdsaSignature.R.ToBigEndianByteArray());
                        concatenated.AddRange(ecdsaSignature.S.ToBigEndianByteArray());
                        signature = concatenated.ToArray();                        
                        break;

                    case KnownOids.rsassa_pss:
                        // in case of rsa ssa pss, the digest is computed along with the verification
                        digestToVerify = signedData.EncapContentInfo.EContent;
                        break;

                    default:
                        digestToVerify = ComputeHash(digestAlgorithmOid, signedData.EncapContentInfo.EContent);
                        signature = signerInfo.Signature.Value;
                        break;
                }

                BigInteger certificateSerialNumber = signerInfo.Sid.IssuerAndSerialNumber.SerialNumber.Value;

                // if SignedAttrs is Present then it should be used (SignedAttrs contains the eContent digest).
                if (signerInfo.isSignedAttrsPresent())
                {
                    foreach (ePassport.Attribute attribute in signerInfo.SignedAttrs.Value)
                    {
                        if (Oids.ParseKnown(attribute.Type.Value.Value) == KnownOids.messageDigest)
                        {
                            byte[] digest = ((List<byte[]>)attribute.Values)[0];

                            // verify that econtent digest is matching
                            if (Utils.Compare(digestToVerify, 0, digest, digest.Length - digestToVerify.Length, digestToVerify.Length) == true)
                            {
                                // since it is matching, let's use the SignedAttrs as input for the digest
                                byte[] dataToHash = Utils.DerEncodeAsByteArray<SignedAttributes>(signerInfo.SignedAttrs);
                                digestToVerify = ComputeHash(digestAlgorithmOid, dataToHash);
                                break;
                            }
                        }

                    }
                }

                foreach (CertificateChoices certChoice in signedData.Certificates.Value)
                {
                    if (Utils.Compare(certChoice.Certificate.TbsCertificate.SerialNumber.Value.ToByteArray(), certificateSerialNumber.ToByteArray()) == true)
                    {
                        bool isSignatureVerifiedSuccessfully = VerifyDigestSignature(
                            certChoice.Certificate.TbsCertificate.SubjectPublicKeyInfo,
                            digestToVerify,
                            digestAlgorithmOid,
                            signature
                            );

                        certificate = certChoice.Certificate;

                        return isSignatureVerifiedSuccessfully;
                    }
                }

            }

            return false;
        }



        #region Extensions

#if USE_BOUNCYCASTLE
        public static Org.BouncyCastle.Math.BigInteger ToBouncyCastleBigInteger(this BigInteger value)
        {
            List<byte> dataList = new List<byte>(value.ToByteArray());
            dataList.Reverse();
            return new Org.BouncyCastle.Math.BigInteger(dataList.ToArray());
        }
#endif

        public static byte[] ToBigEndianByteArray(this BigInteger value)
        {
            List<byte> dataList = new List<byte>(value.ToByteArray());
            dataList.Reverse();
            if (dataList[0] == 0x00)
            {
                dataList.RemoveAt(0);
            }            
            return dataList.ToArray();
        }        

#endregion
    }
}
