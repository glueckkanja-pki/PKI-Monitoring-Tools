
namespace GK.PKIMonitoring.CertWarning
{
    class CertificateRenderer : log4net.ObjectRenderer.IObjectRenderer
    {
        public void RenderObject(log4net.ObjectRenderer.RendererMap rendererMap, object obj, System.IO.TextWriter writer)
        {
            Certificate cert = obj as Certificate;

            if (null == cert)
                return;

            writer.WriteLine();
            writer.WriteLine("RequestID: " + cert.RequestId);
            writer.WriteLine("Request Disp. Message: " + cert.DispMessage);
            writer.WriteLine("Expiration Date: " + cert.ExpirationDate);
            writer.WriteLine("Subject: " + cert.SubjectCommonName);
            writer.WriteLine("Requester Name: " + cert.RequesterName);
            writer.WriteLine("Issued State: " + cert.IssuedState);
            writer.WriteLine("Issued Common Name: " + cert.IssuedCommonName);
            writer.WriteLine("Effective Date: " + cert.CertificateEffectiveDate);
        }
    }
}
