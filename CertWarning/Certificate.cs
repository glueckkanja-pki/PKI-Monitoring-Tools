
namespace GK.PKIMonitoring.CertWarning
{
    class Certificate
    {
        
        private string expirationDate;
        public string ExpirationDate
        {
            get { return expirationDate; }
            set { expirationDate = value; }
        }

        private string groupname;
        public string GroupName
        {
            get { return groupname; }
            set { groupname = value; }
        }
        
        
        private string dispMessage;
        public string DispMessage
        {
            get { return dispMessage; }
            set { dispMessage = value; }
        }

        private string requestId;
        public string RequestId
        {
            get { return requestId; }
            set { requestId = value; }
        }

        private string requesterName;
        public string RequesterName
        {
            get { return requesterName; }
            set { requesterName = value; }
        }



        private string subjectCommonName;
        public string SubjectCommonName 
        {
            get { return subjectCommonName; }
            set { subjectCommonName = value; } 
        }

        private string certificateTemplate;
        public string CertificateTemplate 
        {
            get { return certificateTemplate; }
            set { certificateTemplate = value; } 
        }

        private string revocationDate;
        public string RevocationDate
        {
            get { return revocationDate; }
            set { revocationDate = value; }
        }

        private string effectiveRevocationDate;
        public string EffectiveRevocationDate
        {
            get { return effectiveRevocationDate; }
            set { effectiveRevocationDate = value; }
        }

        private string revocationReason;
        public string RevocationReason
        {
            get { return revocationReason; }
            set { revocationReason = value; }
        }

        private string issuedState;
        public string IssuedState 
        {
            get { return issuedState; }
            set { issuedState = value; }
        }

        private string issuedCommonName;
        public string IssuedCommonName
        {
            get { return issuedCommonName; }
            set { issuedCommonName = value; }
        }

        private string certificateEffectiveDate;
        public string CertificateEffectiveDate
        {
            get { return certificateEffectiveDate; }
            set { certificateEffectiveDate = value; }
        }

        private string binaryCertificate;
        public string BinaryCertificate
        {
            get { return binaryCertificate; }
            set { binaryCertificate = value; }
        }
    }
}
