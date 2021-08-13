
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
    [ASN1Sequence(Name = "Curve", IsSet = false)]
    public class Curve : IASN1PreparedElement 
    {
        
        private FieldElement a_;
        
		[ASN1Element(Name = "a", IsOptional = false, HasTag = false, HasDefaultValue = false)]
        public FieldElement A
        {
            get { return a_; }
            set { a_ = value;  }
        }
  
        private FieldElement b_;
        
		[ASN1Element(Name = "b", IsOptional = false, HasTag = false, HasDefaultValue = false)]
        public FieldElement B
        {
            get { return b_; }
            set { b_ = value;  }
        }
  
        private BitString seed_;
        
        private bool  seed_present = false;
        [ASN1BitString( Name = "" )]
    
		[ASN1Element(Name = "seed", IsOptional = true, HasTag = false, HasDefaultValue = false)]
        public BitString Seed
        {
            get { return seed_; }
            set { seed_ = value; seed_present = true;  }
        }
  
        public bool isSeedPresent()
        {
            return this.seed_present == true;
        }
        

        public void initWithDefaults() 
        {
            
        }

        private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(Curve));
        public IASN1PreparedElementData PreparedData 
        {
            get { return preparedData; }
        }

    }
            
}
