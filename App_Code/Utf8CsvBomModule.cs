using System;
using System.IO;
using System.Web;

/// <summary>
/// Prepends a UTF-8 BOM (EF BB BF) to CSV export responses so that Microsoft
/// Excel on Windows detects the file as UTF-8 and displays Arabic correctly
/// instead of mojibake. The CSV bytes themselves are already UTF-8; only the
/// BOM marker is missing, and that is added here without touching the
/// compiled controller. Registered as a managed module in Web.config.
/// </summary>
public class Utf8CsvBomModule : IHttpModule
{
    public void Init(HttpApplication app)
    {
        app.BeginRequest += OnBeginRequest;
    }

    public void Dispose() { }

    private static void OnBeginRequest(object sender, EventArgs e)
    {
        try
        {
            var context = ((HttpApplication)sender).Context;
            var path = context.Request.Path ?? string.Empty;

            // Only touch export endpoints to keep every other request untouched.
            if (path.IndexOf("Export", StringComparison.OrdinalIgnoreCase) < 0)
                return;

            var response = context.Response;
            response.Filter = new BomFilterStream(response.Filter, response);
        }
        catch
        {
            // Never let the BOM helper break a request.
        }
    }

    private sealed class BomFilterStream : Stream
    {
        private static readonly byte[] Bom = { 0xEF, 0xBB, 0xBF };
        private readonly Stream _inner;
        private readonly HttpResponse _response;
        private bool _inspected;

        public BomFilterStream(Stream inner, HttpResponse response)
        {
            _inner = inner;
            _response = response;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_inspected)
            {
                _inspected = true;

                var contentType = _response.ContentType ?? string.Empty;
                bool isCsv = contentType.IndexOf("csv", StringComparison.OrdinalIgnoreCase) >= 0;
                bool alreadyHasBom = count >= 3
                    && buffer[offset] == 0xEF
                    && buffer[offset + 1] == 0xBB
                    && buffer[offset + 2] == 0xBF;

                if (isCsv && !alreadyHasBom)
                {
                    _inner.Write(Bom, 0, Bom.Length);
                }
            }

            _inner.Write(buffer, offset, count);
        }

        public override void Flush() { _inner.Flush(); }
        public override void Close() { _inner.Close(); base.Close(); }

        public override bool CanRead { get { return false; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }
        public override long Length { get { throw new NotSupportedException(); } }
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
        public override int Read(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
    }
}
