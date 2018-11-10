using System;
using System.Runtime.Serialization;

namespace MusicDb.Abstractions.Exceptions
{
	[Serializable]
	public class DuplicateKeyException : Exception
	{
		public DuplicateKeyException()
		{
		}

		public DuplicateKeyException(string message)
			: base(message)
		{
		}

		public DuplicateKeyException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected DuplicateKeyException(SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base(serializationInfo, streamingContext)
		{
		}
	}
}
