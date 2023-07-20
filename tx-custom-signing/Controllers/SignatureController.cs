using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TXTextControl;
using TXTextControl.Web.MVC.DocumentViewer;
using TXTextControl.Web.MVC.DocumentViewer.Models;

namespace tx_custom_signing.Controllers {
	public class SignatureController : Controller {

		[HttpPost]
		public IActionResult CustomSignature([FromBody] SignatureData data) {

			byte[] bPDF;

			// create temporary ServerTextControl
			using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl()) {
				tx.Create();

				// load the document
				tx.Load(Convert.FromBase64String(data.SignedDocument.Document), TXTextControl.BinaryStreamType.InternalUnicodeFormat);

				var signatureImage = System.Text.Encoding.UTF8.GetString(
				   Convert.FromBase64String(data.SignatureImage));

				var stamp = Encoding.ASCII.GetBytes(signatureImage);

				// create a memory stream from SVG
				using (MemoryStream ms = new MemoryStream(
				   stamp, 0, stamp.Length, writable: false, publiclyVisible: true)) {

					foreach (SignatureField field in tx.SignatureFields) {
						field.Image = new SignatureImage(ms);
					}
				}

				X509Certificate2 cert = new X509Certificate2("App_Data/textcontrolself.pfx", "123");

				var signatureFields = new List<DigitalSignature>();

				foreach (SignatureBox box in data.SignatureBoxes) {
					signatureFields.Add(new DigitalSignature(cert, null, box.Name));
				}

				TXTextControl.SaveSettings saveSettings = new TXTextControl.SaveSettings() {
					CreatorApplication = "Your Application",
					SignatureFields = signatureFields.ToArray()
				};

				// store the PDF in the database or send it to the client
				tx.Save(out bPDF, TXTextControl.BinaryStreamType.AdobePDFA, saveSettings);

				// alternatively, save the PDF to a file
				tx.Save("App_Data/signed.pdf", TXTextControl.StreamType.AdobePDFA, saveSettings);
			}

			// return any value to the client
			return Ok();
		}

		[HttpPost]
		public IActionResult HandleSignature([FromBody] SignatureData data) {

			byte[] bPDF;

			// create temporary ServerTextControl
			using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl()) {
				tx.Create();

				// load the document
				tx.Load(Convert.FromBase64String(data.SignedDocument.Document), TXTextControl.BinaryStreamType.InternalUnicodeFormat);

				//FlattenFormFields(tx);

				X509Certificate2 cert = new X509Certificate2("App_Data/textcontrolself.pfx", "123");

				var signatureFields = new List<DigitalSignature>();

				foreach (SignatureBox box in data.SignatureBoxes) {
					signatureFields.Add(new DigitalSignature(cert, null, box.Name));
				}

				TXTextControl.SaveSettings saveSettings = new TXTextControl.SaveSettings() {
					CreatorApplication = "Your Application",
					SignatureFields = signatureFields.ToArray()
				};

				// store the PDF in the database or send it to the client
				tx.Save(out bPDF, TXTextControl.BinaryStreamType.AdobePDFA, saveSettings);

				// alternatively, save the PDF to a file
				tx.Save("App_Data/signed.pdf", TXTextControl.StreamType.AdobePDFA, saveSettings);
			}

			// return any value to the client
			return Ok();
		}
	}
}
