using System;
using System.Runtime.Serialization;

namespace MusicDb.Abstractions.Exceptions
{
	[Serializable]
	public class NotFoundException : Exception
	{
		public NotFoundException()
		{
		}

		public NotFoundException(string message)
			: base(message)
		{
		}

		public NotFoundException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected NotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base(serializationInfo, streamingContext)
		{
		}
	}
}
