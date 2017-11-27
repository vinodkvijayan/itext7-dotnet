using System;
using System.Collections.Generic;
using iText.IO.Util;

namespace iText.Kernel {
    /// <summary>Exception class for License-key version exceptions throw in the Version class</summary>
    public class LicenseVersionException : Exception {
        public const String NO_I_TEXT7_LICENSE_IS_LOADED_BUT_AN_I_TEXT5_LICENSE_IS_LOADED = "No iText7 License is loaded but an iText5 license is loaded.";

        public const String THE_MAJOR_VERSION_OF_THE_LICENSE_0_IS_LOWER_THAN_THE_MAJOR_VERSION_1_OF_THE_CORE_LIBRARY
             = "The major version of the license ({0}) is lower than the major version ({1}) of the Core library.";

        public const String THE_MAJOR_VERSION_OF_THE_LICENSE_0_IS_HIGHER_THAN_THE_MAJOR_VERSION_1_OF_THE_CORE_LIBRARY
             = "The major version of the license ({0}) is higher than the major version ({1}) of the Core library.";

        public const String THE_MINOR_VERSION_OF_THE_LICENSE_0_IS_LOWER_THAN_THE_MINOR_VERSION_1_OF_THE_CORE_LIBRARY
             = "The minor version of the license ({0}) is lower than the minor version ({1}) of the Core library.";

        public const String THE_MINOR_VERSION_OF_THE_LICENSE_0_IS_HIGHER_THAN_THE_MINOR_VERSION_1_OF_THE_CORE_LIBRARY
             = "The minor version of the license ({0}) is higher than the minor version ({1}) of the Core library.";

        public const String VERSION_STRING_IS_EMPTY_AND_CANNOT_BE_PARSED = "Version string is empty and cannot be parsed.";

        public const String MAJOR_VERSION_IS_NOT_NUMERIC = "Major version is not numeric";

        public const String MINOR_VERSION_IS_NOT_NUMERIC = "Minor version is not numeric";

        public const String UNKNOWN_EXCEPTION_WHEN_CHECKING_LICENSE_VERSION = "Unknown Exception when checking License version";

        public const String LICENSE_FILE_NOT_LOADED = "License file not loaded.";

        /// <summary>Object for more details</summary>
        protected internal Object @object;

        private IList<Object> messageParams;

        /// <summary>Creates a new instance of PdfException.</summary>
        /// <param name="message">the detail message.</param>
        public LicenseVersionException(String message)
            : base(message) {
        }

        /// <summary>Creates a new instance of PdfException.</summary>
        /// <param name="cause">
        /// the cause (which is saved for later retrieval by
        /// <see cref="System.Exception.InnerException()"/>
        /// method).
        /// </param>
        public LicenseVersionException(Exception cause)
            : this(UNKNOWN_EXCEPTION_WHEN_CHECKING_LICENSE_VERSION, cause) {
        }

        /// <summary>Creates a new instance of PdfException.</summary>
        /// <param name="message">the detail message.</param>
        /// <param name="obj">an object for more details.</param>
        public LicenseVersionException(String message, Object obj)
            : this(message) {
            this.@object = obj;
        }

        /// <summary>Creates a new instance of PdfException.</summary>
        /// <param name="message">the detail message.</param>
        /// <param name="cause">
        /// the cause (which is saved for later retrieval by
        /// <see cref="System.Exception.InnerException()"/>
        /// method).
        /// </param>
        public LicenseVersionException(String message, Exception cause)
            : base(message, cause) {
        }

        /// <summary>Creates a new instance of PdfException.</summary>
        /// <param name="message">the detail message.</param>
        /// <param name="cause">
        /// the cause (which is saved for later retrieval by
        /// <see cref="System.Exception.InnerException()"/>
        /// method).
        /// </param>
        /// <param name="obj">an object for more details.</param>
        public LicenseVersionException(String message, Exception cause, Object obj)
            : this(message, cause) {
            this.@object = obj;
        }

        public override String Message {
            get {
                if (messageParams == null || messageParams.Count == 0) {
                    return base.Message;
                }
                else {
                    return MessageFormatUtil.Format(base.Message, GetMessageParams());
                }
            }
        }

        /// <summary>Sets additional params for Exception message.</summary>
        /// <param name="messageParams">additional params.</param>
        /// <returns>object itself.</returns>
        public virtual iText.Kernel.LicenseVersionException SetMessageParams(params Object[] messageParams) {
            this.messageParams = new List<Object>();
            this.messageParams.AddAll(messageParams);
            return this;
        }

        /// <summary>Gets additional params for Exception message.</summary>
        protected internal virtual Object[] GetMessageParams() {
            Object[] parameters = new Object[messageParams.Count];
            for (int i = 0; i < messageParams.Count; i++) {
                parameters[i] = messageParams[i];
            }
            return parameters;
        }
    }
}
