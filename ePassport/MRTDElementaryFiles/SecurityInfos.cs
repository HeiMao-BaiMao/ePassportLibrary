
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
    [ASN1BoxedType(Name = "SecurityInfos")]
    public class SecurityInfos : IASN1PreparedElement 
    {

        private System.Collections.Generic.ICollection<SecurityInfo> val = null; 
        
        
        [ASN1SequenceOf(Name = "SecurityInfos", IsSetOf = true)]

        public System.Collections.Generic.ICollection<SecurityInfo> Value
        {
            get { return val; }
            set { val = value; }
        }
        
        public void initValue() 
        {
            this.Value = new System.Collections.Generic.List<SecurityInfo>();
        }
        
        public void Add(SecurityInfo item) 
        {
            this.Value.Add(item);
        }

        public void initWithDefaults()
        {
        }

        private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(SecurityInfos));
        public IASN1PreparedElementData PreparedData 
        {
            get { return preparedData; }
        }

    }
            
}
