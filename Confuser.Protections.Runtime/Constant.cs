﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace Confuser.Runtime {
	// ReSharper disable once UnusedType.Global
	internal static class Constant {
		private static byte[] b;

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Injected code")]
		[SuppressMessage("Code Quality", "IDE0051:RemoveUnusdPrivateMembers", Justification = "Injected code")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private static void Initialize() {
			var l = (uint)Mutation.KeyI0;
			var q = Mutation.Placeholder(new uint[Mutation.KeyI0]);

			var k = new uint[0x10];
			var n = (uint)Mutation.KeyI1;
			for (var i = 0; i < 0x10; i++) {
				n ^= n >> 12;
				n ^= n << 25;
				n ^= n >> 27;
				k[i] = n;
			}

			int s = 0, d = 0;
			var w = new uint[0x10];
			var o = new byte[l * 4];
			while (s < l) {
				Array.Copy(q, s, w, 0, 0x10);

				Mutation.Crypt(w, k);
				for (var j = 0; j < 0x10; j++) {
					var e = w[j];
					o[d++] = (byte)e;
					o[d++] = (byte)(e >> 8);
					o[d++] = (byte)(e >> 16);
					o[d++] = (byte)(e >> 24);
					k[j] ^= e;
				}

				s += 0x10;
			}

			b = CompressionService.Decompress(o);
		}

		internal static T Get<T>(int id) {
			// op_equality is not available on .NET Framework 2.0 and older. To ensure compatibility,
			// we'll be using equals.
			if (Assembly.GetExecutingAssembly().Equals(Assembly.GetCallingAssembly())) {
				id = Mutation.Placeholder(id);
				int t = (int)((uint)id >> 30);

				T ret;
				id = (id & 0x3fffffff) << 2;

				if (t == Mutation.KeyI0) {
					int l = b[id] | (b[id+1] << 8) | (b[id+2] << 16) | (b[id+3] << 24);
					ret = (T)(object)string.Intern(Encoding.UTF8.GetString(b, id+4, l));
				}
				// NOTE: Assume little-endian
				else if (t == Mutation.KeyI1) {
					var v = new T[1];
					Buffer.BlockCopy(b, id, v, 0, Mutation.Value<int>());
					ret = v[0];
				}
				else if (t == Mutation.KeyI2) {
					int s = b[id] | (b[id+1] << 8) | (b[id+2] << 16) | (b[id+3] << 24);
					int l = b[id+4] | (b[id+5] << 8) | (b[id+6] << 16) | (b[id+7] << 24);
					Array v = Array.CreateInstance(typeof(T).GetElementType(), l);
					Buffer.BlockCopy(b, id+8, v, 0, s - 4);
					ret = (T)(object)v;
				}
				else
					ret = default(T);

				return ret;
			}

			return default(T);
		}
	}

	internal struct CFGCtx {
		uint A;
		uint B;
		uint C;
		uint D;

		public CFGCtx(uint seed) {
			A = seed *= 0x21412321;
			B = seed *= 0x21412321;
			C = seed *= 0x21412321;
			D = seed *= 0x21412321;
		}

		public uint Next(byte f, uint q) {
			if ((f & 0x80) != 0) {
				switch (f & 0x3) {
					case 0:
						A = q;
						break;
					case 1:
						B = q;
						break;
					case 2:
						C = q;
						break;
					case 3:
						D = q;
						break;
				}
			}
			else {
				switch (f & 0x3) {
					case 0:
						A ^= q;
						break;
					case 1:
						B += q;
						break;
					case 2:
						C ^= q;
						break;
					case 3:
						D -= q;
						break;
				}
			}

			switch ((f >> 2) & 0x3) {
				case 0:
					return A;
				case 1:
					return B;
				case 2:
					return C;
			}

			return D;
		}
	}
}
