using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizedTextEditor.Classes
{
	/// <summary>
	/// A 16-byte identifier for all objects in the application. This Id is generated using the current time and a randomly generated number.
	/// </summary>
	public struct Id
	{
		byte[] _id;

		public Id()
		{
			long ticksNow = DateTime.Now.Ticks;

			Random random = new Random();
			byte[] randomBytes = new byte[8];
			random.NextBytes(randomBytes);

			_id = new byte[16];
			Array.Copy(BitConverter.GetBytes(ticksNow), 0, _id, 0, 8);
			Array.Copy(randomBytes, 0, _id, 8, 8);
		}

		Id(byte[] id)
		{
			if (id.Length != 16)
				throw new ArgumentException("Id must be 16 bytes long");

			_id = id;
		}

		public static Id FromByteArray(byte[] id)
		{
			return new Id(id);
		}

		public string ToHexString()
		{
			return BitConverter.ToString(_id).Replace("-", "");
		}

		public bool Equals(Id other)
		{
			for (int i = 0; i < 16; i++)
			{
				if (_id[i] != other._id[i])
					return false;
			}
			return true;
		}

		public static bool operator ==(Id left, Id right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Id left, Id right)
		{
			return !left.Equals(right);
		}

		public override bool Equals(object? obj)
		{
			if (obj is Id)
				return Equals((Id)obj);
			return false;
		}

		public override int GetHashCode()
		{
			return _id.GetHashCode();
		}

		public static Id EMPTY_ID = new Id(new byte[16]);
	}
}
