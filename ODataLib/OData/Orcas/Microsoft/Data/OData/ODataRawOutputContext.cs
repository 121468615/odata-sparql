//   Copyright 2011 Microsoft Corporation
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace Microsoft.Data.OData
{
    #region Namespaces
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.Data.Edm;
    #endregion Namespaces

    /// <summary>
    /// RAW format output context. Used by RAW values and batch.
    /// </summary>
    internal sealed class ODataRawOutputContext : ODataOutputContext
    {
        /// <summary>The encoding to use for the output.</summary>
        private Encoding encoding;

        /// <summary>The message output stream.</summary>
        private Stream messageOutputStream;

        /// <summary>The asynchronous output stream if we're writing asynchronously.</summary>
        private AsyncBufferedStream asynchronousOutputStream;

        /// <summary>The output stream to write to (both sync and async cases).</summary>
        private Stream outputStream;

        /// <summary>The text writer created for the output stream. If we have any.</summary>
        private TextWriter textWriter;

        /// <summary>Listener to notify when writing in-stream errors.</summary>
        private IODataOutputInStreamErrorListener outputInStreamErrorListener;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="format">The format for this output context.</param>
        /// <param name="messageStream">The message stream to write the payload to.</param>
        /// <param name="encoding">The encoding to use for the payload.</param>
        /// <param name="messageWriterSettings">Configuration settings of the OData writer.</param>
        /// <param name="writingResponse">true if writing a response message; otherwise false.</param>
        /// <param name="synchronous">true if the output should be written synchronously; false if it should be written asynchronously.</param>
        /// <param name="model">The model to use.</param>
        /// <param name="urlResolver">The optional URL resolver to perform custom URL resolution for URLs written to the payload.</param>
        private ODataRawOutputContext(
            ODataFormat format,
            Stream messageStream,
            Encoding encoding,
            ODataMessageWriterSettings messageWriterSettings,
            bool writingResponse,
            bool synchronous,
            IEdmModel model,
            IODataUrlResolver urlResolver)
            : base(format, messageWriterSettings, writingResponse, synchronous, model, urlResolver)
        {
            try
            {
                this.messageOutputStream = messageStream;
                this.encoding = encoding;

                if (synchronous)
                {
                    this.outputStream = messageStream;
                }
                else
                {
                    this.asynchronousOutputStream = new AsyncBufferedStream(messageStream);
                    this.outputStream = this.asynchronousOutputStream;
                }
            }
            catch
            {
                messageStream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// The output stream to write the payload to.
        /// </summary>
        internal Stream OutputStream
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.outputStream;
            }
        }

        /// <summary>
        /// The text writer to use to write text into the payload.
        /// </summary>
        /// <remarks>InitializeTextWriter must be called before this is used.</remarks>
        internal TextWriter TextWriter
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.textWriter;
            }
        }

        /// <summary>
        /// Create RAW output context.
        /// </summary>
        /// <param name="format">The format to create the output context for.</param>
        /// <param name="message">The message to use.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <param name="messageWriterSettings">Configuration settings of the OData writer.</param>
        /// <param name="writingResponse">true if writing a response message; otherwise false.</param>
        /// <param name="model">The model to use.</param>
        /// <param name="urlResolver">The optional URL resolver to perform custom URL resolution for URLs written to the payload.</param>
        /// <returns>The newly created output context.</returns>
        internal static ODataOutputContext Create(
            ODataFormat format,
            ODataMessage message,
            Encoding encoding,
            ODataMessageWriterSettings messageWriterSettings,
            bool writingResponse,
            IEdmModel model,
            IODataUrlResolver urlResolver)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(format == ODataFormat.RawValue || format == ODataFormat.Batch, "This method only supports the RAW value or batch format.");
            Debug.Assert(message != null, "message != null");
            Debug.Assert(messageWriterSettings != null, "messageWriterSettings != null");

            Stream messageStream = message.GetStream();
            return new ODataRawOutputContext(
                format,
                messageStream,
                encoding,
                messageWriterSettings,
                writingResponse,
                true,
                model,
                urlResolver);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously create RAW output context.
        /// </summary>
        /// <param name="format">The format to create the outpu context for.</param>
        /// <param name="message">The message to use.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <param name="messageWriterSettings">Configuration settings of the OData writer.</param>
        /// <param name="writingResponse">true if writing a response message; otherwise false.</param>
        /// <param name="model">The model to use.</param>
        /// <param name="urlResolver">The optional URL resolver to perform custom URL resolution for URLs written to the payload.</param>
        /// <returns>Task which when completed returns the newly created output context.</returns>
        internal static Task<ODataOutputContext> CreateAsync(
            ODataFormat format,
            ODataMessage message,
            Encoding encoding,
            ODataMessageWriterSettings messageWriterSettings,
            bool writingResponse,
            IEdmModel model,
            IODataUrlResolver urlResolver)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(format == ODataFormat.RawValue || format == ODataFormat.Batch, "This method only supports the RAW value or batch format.");
            Debug.Assert(message != null, "message != null");
            Debug.Assert(messageWriterSettings != null, "messageWriterSettings != null");

            return message.GetStreamAsync()
                .FollowOnSuccessWith(
                    (streamTask) => (ODataOutputContext)new ODataRawOutputContext(
                        format,
                        streamTask.Result,
                        encoding,
                        messageWriterSettings,
                        writingResponse,
                        false,
                        model,
                        urlResolver));
        }
#endif

        /// <summary>
        /// Synchronously flush the writer.
        /// </summary>
        internal void Flush()
        {
            DebugUtils.CheckNoExternalCallers();
            this.AssertSynchronous();

            if (this.textWriter != null)
            {
                // The TextWriter.Flush (Which is in fact StreamWriter.Flush) will call the underlying Stream.Flush.
                // In the synchronous case the underlying stream is the message stream itself, which will then Flush as well.
                this.textWriter.Flush();
            }
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously flush the writer.
        /// </summary>
        /// <returns>Task which represents the pending flush operation.</returns>
        /// <remarks>The method should not throw directly if the flush operation itself fails, it should instead return a faulted task.</remarks>
        internal Task FlushAsync()
        {
            DebugUtils.CheckNoExternalCallers();
            this.AssertAsynchronous();

            return TaskUtils.GetTaskForSynchronousOperationReturningTask(
                () =>
                {
                    if (this.textWriter != null)
                    {
                        // The TextWriter.Flush (Which is in fact StreamWriter.Flush) will call the underlying Stream.Flush.
                        // In the async case the underlying stream is the async buffered stream, which ignores Flush call.
                        this.textWriter.Flush();
                    }

                    Debug.Assert(this.asynchronousOutputStream != null, "In async writing we must have the async buffered stream.");
                    return this.asynchronousOutputStream.FlushAsync();
                })
                .FollowOnSuccessWithTask((asyncBufferedStreamFlushTask) => this.messageOutputStream.FlushAsync());
        }
#endif

        /// <summary>
        /// Writes an <see cref="ODataError"/> into the message payload.
        /// </summary>
        /// <param name="error">The error to write.</param>
        /// <param name="includeDebugInformation">
        /// A flag indicating whether debug information (e.g., the inner error from the <paramref name="error"/>) should 
        /// be included in the payload. This should only be used in debug scenarios.
        /// </param>
        /// <remarks>
        /// This method is called if the ODataMessageWriter.WriteError is called once some other
        /// write operation has already started.
        /// The method should write the in-stream error representation for the specific format into the current payload.
        /// Before the method is called no flush is performed on the output context or any active writer.
        /// It is the responsibility of this method to flush the output before the method returns.
        /// </remarks>
        internal override void WriteInStreamError(ODataError error, bool includeDebugInformation)
        {
            DebugUtils.CheckNoExternalCallers();
            if (this.outputInStreamErrorListener != null)
            {
                this.outputInStreamErrorListener.OnInStreamError();
            }

            throw new ODataException(Strings.ODataMessageWriter_CannotWriteInStreamErrorForRawValues);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Writes an <see cref="ODataError"/> into the message payload.
        /// </summary>
        /// <param name="error">The error to write.</param>
        /// <param name="includeDebugInformation">
        /// A flag indicating whether debug information (e.g., the inner error from the <paramref name="error"/>) should 
        /// be included in the payload. This should only be used in debug scenarios.
        /// </param>
        /// <returns>Task which represents the pending write operation.</returns>
        /// <remarks>
        /// This method is called if the ODataMessageWriter.WriteError is called once some other
        /// write operation has already started.
        /// The method should write the in-stream error representation for the specific format into the current payload.
        /// Before the method is called no flush is performed on the output context or any active writer.
        /// It is the responsibility of this method to make sure that all the data up to this point are written before
        /// the in-stream error is written.
        /// It is the responsibility of this method to flush the output before the task finishes.
        /// </remarks>
        internal override Task WriteInStreamErrorAsync(ODataError error, bool includeDebugInformation)
        {
            DebugUtils.CheckNoExternalCallers();
            if (this.outputInStreamErrorListener != null)
            {
                this.outputInStreamErrorListener.OnInStreamError();
            }

            throw new ODataException(Strings.ODataMessageWriter_CannotWriteInStreamErrorForRawValues);
        }
#endif

        /// <summary>
        /// Creates an <see cref="ODataBatchWriter" /> to write a batch of requests or responses.
        /// </summary>
        /// <param name="batchBoundary">The boundary string for the batch structure itself.</param>
        /// <returns>The created batch writer.</returns>
        /// <remarks>We don't plan to make this public!</remarks>
        /// <remarks>The write must flush the output when it's finished (inside the last Write call).</remarks>
        internal override ODataBatchWriter CreateODataBatchWriter(string batchBoundary)
        {
            DebugUtils.CheckNoExternalCallers();
            this.AssertSynchronous();

            return this.CreateODataBatchWriterImplementation(batchBoundary);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously creates an <see cref="ODataBatchWriter" /> to write a batch of requests or responses.
        /// </summary>
        /// <param name="batchBoundary">The boundary string for the batch structure itself.</param>
        /// <returns>A running task for the created batch writer.</returns>
        /// <remarks>We don't plan to make this public!</remarks>
        /// <remarks>The write must flush the output when it's finished (inside the last Write call).</remarks>
        internal override Task<ODataBatchWriter> CreateODataBatchWriterAsync(string batchBoundary)
        {
            DebugUtils.CheckNoExternalCallers();
            this.AssertAsynchronous();

            return TaskUtils.GetTaskForSynchronousOperation(() => this.CreateODataBatchWriterImplementation(batchBoundary));
        }
#endif

        /// <summary>
        /// Writes a single value as the message body.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <remarks>It is the responsibility of this method to flush the output before the method returns.</remarks>
        internal override void WriteValue(object value)
        {
            DebugUtils.CheckNoExternalCallers();
            this.AssertSynchronous();

            this.WriteValueImplementation(value);
            this.Flush();
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously writes a single value as the message body.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>A running task representing the writing of the value.</returns>
        /// <remarks>It is the responsibility of this method to flush the output before the task finishes.</remarks>
        internal override Task WriteValueAsync(object value)
        {
            DebugUtils.CheckNoExternalCallers();
            this.AssertAsynchronous();

            return TaskUtils.GetTaskForSynchronousOperationReturningTask(
                () =>
                {
                    this.WriteValueImplementation(value);
                    return this.FlushAsync();
                });
        }
#endif

        /// <summary>
        /// Initialized a new text writer over the message payload stream.
        /// </summary>
        /// <remarks>This can only be called if the text writer was not yet initialized or it has been closed.
        /// It can be called several times with CloseTextWriter calls in between though.</remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "We create a NonDisposingStream which doesn't need to be disposed, even though it's IDisposable.")]
        internal void InitializeTextWriter()
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(this.textWriter == null, "The text writer has already been initialized.");

            // We must create the text writer over a stream which will ignore Dispose, since we need to be able to Dispose 
            // the writer without disposing the underlying message stream.
            Stream nonDisposingStream;
            if (MessageStreamWrapper.IsNonDisposingStream(this.outputStream) || this.outputStream is AsyncBufferedStream)
            {
                // AsynBufferedStream ignores Dispose
                nonDisposingStream = this.outputStream;
            }
            else
            {
                nonDisposingStream = MessageStreamWrapper.CreateNonDisposingStream(this.outputStream);
            }

            this.textWriter = new StreamWriter(nonDisposingStream, this.encoding);
        }

        /// <summary>
        /// Closes the text writer.
        /// </summary>
        internal void CloseTextWriter()
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(this.textWriter != null, "The text writer has not been initialized yet.");

            this.textWriter.Dispose();
            this.textWriter = null;
        }

        /// <summary>
        /// Verifies the output context was not yet disposed, fails otherwise.
        /// </summary>
        internal void VerifyNotDisposed()
        {
            DebugUtils.CheckNoExternalCallers();
            
            if (this.messageOutputStream == null)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        /// <summary>
        /// Flushes all buffered data to the underlying stream synchronously.
        /// </summary>
        internal void FlushBuffers()
        {
            DebugUtils.CheckNoExternalCallers();

            if (this.asynchronousOutputStream != null)
            {
                this.asynchronousOutputStream.FlushSync();
            }
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Flushes all buffered data to the underlying stream asynchronously.
        /// </summary>
        /// <returns>Task which represents the pending operation.</returns>
        internal Task FlushBuffersAsync()
        {
            DebugUtils.CheckNoExternalCallers();

            if (this.asynchronousOutputStream != null)
            {
                return this.asynchronousOutputStream.FlushAsync();
            }
            else
            {
                return TaskUtils.CompletedTask;
            }
        }
#endif

        /// <summary>
        /// Perform the actual cleanup work.
        /// </summary>
        /// <param name="disposing">If 'true' this method is called from user code; if 'false' it is called by the runtime.</param>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "textWriter", Justification = "We intentionally don't dispose textWriter, we instead dispose the underlying stream manually.")]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try
            {
                if (this.messageOutputStream != null)
                {
                    if (this.textWriter != null)
                    {
                        this.textWriter.Flush();
                    }

                    // In the async case the underlying stream is the async buffered stream, so we have to flush that explicitely.
                    if (this.asynchronousOutputStream != null)
                    {
                        this.asynchronousOutputStream.FlushSync();
                        this.asynchronousOutputStream.Dispose();
                    }

                    // Dipose the message stream (note that we OWN this stream, so we always dispose it).
                    this.messageOutputStream.Dispose();
                }
            }
            finally
            {
                this.messageOutputStream = null;
                this.asynchronousOutputStream = null;
                this.outputStream = null;
                this.textWriter = null;
            }
        }

        /// <summary>
        /// Writes a single value as the message body.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <remarks>Once the method returns all the data should be written, the only other call after this will be Dispose on the output context.</remarks>
        private void WriteValueImplementation(object value)
        {
            byte[] binaryValue = value as byte[];
            if (binaryValue != null)
            {
                this.WriteBinaryValue(binaryValue);
            }
            else
            {
                this.WriteRawValue(value);
            }
        }

        /// <summary>
        /// Creates a batch writer.
        /// </summary>
        /// <param name="batchBoundary">The boundary string for the batch structure itself.</param>
        /// <returns>The newly created batch writer.</returns>
        private ODataBatchWriter CreateODataBatchWriterImplementation(string batchBoundary)
        {
            // Batch writer needs the default encoding to not use the preamble.
            this.encoding = this.encoding ?? MediaTypeUtils.EncodingUtf8NoPreamble;
            ODataBatchWriter batchWriter = new ODataBatchWriter(this, batchBoundary);
            this.outputInStreamErrorListener = batchWriter;
            return batchWriter;
        }

        /// <summary>
        /// Writes the specified binary value to the output.
        /// </summary>
        /// <param name="bytes">The binary value to write.</param>
        private void WriteBinaryValue(byte[] bytes)
        {
            Debug.Assert(bytes != null, "bytes != null");

            // write the bytes directly
            this.OutputStream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Converts the specified <paramref name="value"/> into its raw format and writes it to the output.
        /// The value has to be of primitive type.
        /// </summary>
        /// <param name="value">The (non-binary) value to write.</param>
        /// <remarks>We do not accept binary values here; WriteBinaryValue should be used for binary data.</remarks>
        private void WriteRawValue(object value)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(!(value is byte[]), "!(value is byte[])");

            string valueAsString;
            if (!AtomValueUtils.TryConvertPrimitiveToString(value, out valueAsString))
            {
                // throw an exception because the value is not primitive
                throw new ODataException(Strings.ODataUtils_CannotConvertValueToRawPrimitive(value.GetType().FullName));
            }

            this.InitializeTextWriter();
            this.TextWriter.Write(valueAsString);
        }
    }
}
