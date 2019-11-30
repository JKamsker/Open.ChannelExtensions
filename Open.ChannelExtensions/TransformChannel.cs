﻿using System;
using System.Diagnostics.Contracts;
using System.Threading.Channels;

namespace Open.ChannelExtensions
{
	/// <summary>
	/// A channel wrapper that takes the provided channel and transforms them on demand when being read.
	/// </summary>
	/// <typeparam name="TWrite">Specifies the type of data that may be written to the channel.</typeparam>
	/// <typeparam name="TRead">Specifies the type of data that may be read from the channel.</typeparam>
	public class TransformChannel<TWrite, TRead> : Channel<TWrite, TRead>
	{
		/// <summary>
		/// Creates a channel wrapper that takes the provided channel and transforms them on demand when being read.
		/// </summary>
		/// <param name="source">The channel containing the source data.</param>
		/// <param name="transform">The transform function to be applied to the results when being read.</param>
		public TransformChannel(Channel<TWrite, TWrite> source, Func<TWrite, TRead> transform)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			if (transform is null) throw new ArgumentNullException(nameof(transform));
			Contract.EndContractBlock();

			Writer = source.Writer;
			Reader = source.Reader.Transform(transform);
		}
	}
}
