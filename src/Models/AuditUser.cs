using System;

namespace DotNet.Perf.Models
{
    public class AuditUser : IEquatable<AuditUser>
    {
        internal AuditUser()
        {
        }

        public AuditUser(string userId, string firstName, string lastName)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
        }

        public string UserId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public string FullName()
        {
            if (string.IsNullOrWhiteSpace(FirstName)) return LastName;
            if (string.IsNullOrWhiteSpace(LastName)) return FirstName;
            return FirstName + " " + LastName;
        }

        public string FullNameFormal()
        {
            if (string.IsNullOrWhiteSpace(FirstName)) return LastName;
            if (string.IsNullOrWhiteSpace(LastName)) return FirstName;
            return LastName + ", " + FirstName;
        }

        public bool Equals(AuditUser other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(UserId, other.UserId) && string.Equals(FirstName, other.FirstName) &&
                   string.Equals(LastName, other.LastName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AuditUser) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (UserId != null ? UserId.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (FirstName != null ? FirstName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (LastName != null ? LastName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(AuditUser left, AuditUser right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AuditUser left, AuditUser right)
        {
            return !Equals(left, right);
        }
    }
}
