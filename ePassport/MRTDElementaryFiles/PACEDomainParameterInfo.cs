
//
// This file was generated by the BinaryNotes compiler (created by Abdulla Abdurakhmanov, modified by Sylvain Prevost).
// See http://bnotes.sourceforge.net 
// Any modifications to this file will be lost upon recompilation of the source ASN.1. 
//

using System;
using System.Numerics;

using org.bn.attributes;
using org.bn.attributes.constraints;
using org.bn.coders;
using org.bn.types;
using org.bn;

namespace ePassport {


    [ASN1PreparedElement]
    [ASN1Sequence(Name = "PACEDomainParameterInfo", IsSet = false)]
    public class PACEDomainParameterInfo : IASN1PreparedElement 
    {
        
        private ObjectIdentifier protocol_;
        [ASN1ObjectIdentifier( Name = "" )]
    
		[ASN1Element(Name = "protocol", IsOptional = false, HasTag = false, HasDefaultValue = false)]
        public ObjectIdentifier Protocol
        {
            get { return protocol_; }
            set { protocol_ = value;  }
        }
  
        private AlgorithmIdentifier domainParameter_;
        
		[ASN1Element(Name = "domainParameter", IsOptional = false, HasTag = false, HasDefaultValue = false)]
        public AlgorithmIdentifier DomainParameter
        {
            get { return domainParameter_; }
            set { domainParameter_ = value;  }
        }
  
        private BigInteger parameterId_;
        
        private bool  parameterId_present = false;
        [ASN1Integer( Name = "" )]
    
		[ASN1Element(Name = "parameterId", IsOptional = true, HasTag = false, HasDefaultValue = false)]
        public BigInteger ParameterId
        {
            get { return parameterId_; }
            set { parameterId_ = value; parameterId_present = true;  }
        }
  
        public bool isParameterIdPresent()
        {
            return this.parameterId_present == true;
        }
        

        public void initWithDefaults() 
        {
            
        }

        private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(PACEDomainParameterInfo));
        public IASN1PreparedElementData PreparedData 
        {
            get { return preparedData; }
        }

    }
            
}
