namespace Cedar
{
    using System;
    using Cedar.Testing;
    using Xunit;

    public class MessageEqualityComparerTests
    {
        [Fact]
        public void compare_complex_messages()
        {
            Assert.True(MessageEqualityComparer.Instance.Equals(
                new TestMessage
                {
                    AGuid = Guid.Empty,
                    Value = "Value",
                    Date = new DateTime(2000, 1, 1),
                    Items = new[]
                    {
                        new TestMessageItem
                        {
                            Value = "Value"
                        },
                        new TestMessageItem
                        {
                            Value = "AnotherValue"
                        }
                    }
                }, new TestMessage
                {
                    AGuid = Guid.Empty,
                    Value = "Value",
                    Date = new DateTime(2000, 1, 1),
                    Items = new[]
                    {
                        new TestMessageItem
                        {
                            Value = "Value"
                        },
                        new TestMessageItem
                        {
                            Value = "AnotherValue"
                        }
                    }
                }));
        }

        private class TestMessage
        {
            public DateTime Date { get; set; }
            public string Value { get; set; }
            public Guid AGuid { get; set; }
            public TestMessageItem[] Items { get; set; }
        }

        private class TestMessageItem
        {
            public string Value { get; set; }
        }
    }
}