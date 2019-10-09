using System;

namespace DotNet.Perf.Models
{
    public class TestProduct
    {
        internal TestProduct()
        {
        }

        public TestProduct(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                Id = Guid.NewGuid().ToString();
            }
            
            Name = name;
            Description = description;
            Stamp = new Stamp(
                new AuditUser("users/1", "User 1", "CreatedBy"),
                new AuditUser("users/2", "User 2", "CreatedOnBehalfOf"),
                DateTimeOffset.Now,
                new AuditUser("users/3", "User 3", "LastModifiedBy"),
                new AuditUser("users/4", "User 4", "LastModifiedOnBehalfOf"),
                DateTimeOffset.Now);
        }

        public void SetId(string id)
        {
            Id = id;
        }

        public void ChangeName(string name)
        {
            Name = name;
        }

        public void ChangeDescription(string description)
        {
            Description = description;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Stamp Stamp { get; private set; }
    }
}
