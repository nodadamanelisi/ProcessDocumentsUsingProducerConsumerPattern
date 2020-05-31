using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessDocumentsUsingProducerConsumerPattern
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            /** 
             * https://www.dotnetcurry.com/patterns-practices/1407/producer-consumer-pattern-dotnet-csharp
             * */
            var start = DateTime.Now;
            ProcessDocumentsUsingProducerConsumerPattern();
            var milliSeconds = (DateTime.Now - start).TotalMilliseconds.ToString();
            var seconds = (DateTime.Now - start).TotalSeconds.ToString();
            MessageBox.Show("MilliSeconds: " + milliSeconds + " Seconds: " + seconds, "Done");
        }
        string[] GetDocumentIdsToProcess()
        {
            return Directory.GetFiles("*****");
        }
        public void ProcessDocumentsUsingProducerConsumerPattern()
        {
            string[] documentIds = GetDocumentIdsToProcess();

            var inputQueue = CreateInputQueue(documentIds);

            var queue = new BlockingCollection<string>(500);

            var consumer = Task.Run(() =>
            {
                foreach (var translatedDocument in queue.GetConsumingEnumerable())
                {
                    SaveDocumentToDestinationStore(translatedDocument);
                }
            });

            var producers = Enumerable.Range(0, 7)
                .Select(_ => Task.Run(() =>
                {
                    foreach (var documentId in inputQueue.GetConsumingEnumerable())
                    {
                        var document = ReadAndTranslateDocument(documentId);
                        queue.Add(document);
                    }
                }))
                .ToArray();

            Task.WaitAll(producers);

            queue.CompleteAdding();

            consumer.Wait();
        }

        private string ReadAndTranslateDocument(string documentId)
        {
            File.WriteAllText(documentId, "Some Content.");
            return documentId;
        }

        private void SaveDocumentToDestinationStore(string translatedDocument)
        {
            var destinationPath = @"*****";
            var destination = $"{destinationPath}{translatedDocument.Substring(translatedDocument.LastIndexOf('\\'))}";
            File.Copy(translatedDocument, destination, true);
        }

        private BlockingCollection<string> CreateInputQueue(string[] documentIds)
        {
            var inputQueue = new BlockingCollection<string>();

            foreach (var id in documentIds)
                inputQueue.Add(id);

            inputQueue.CompleteAdding();

            return inputQueue;
        }

    }
}
