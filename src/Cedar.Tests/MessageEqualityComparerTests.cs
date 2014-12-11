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
                    AGuid2 = Guid.NewGuid(),
                    Value = "Value",
                    Date = new DateTime(2000, 1, 1),
                    Date2 = new DateTime(2000, 1, 1),
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
                    AGuid2 = Any.Guid,
                    Value = "Value",
                    Date = new DateTime(2000, 1, 1),
                    Date2 = Any.Date,
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

        [Fact]
        public void compare_differencing_complex_messages()
        {
            Assert.False(MessageEqualityComparer.Instance.Equals(
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
                    Value = "different",
                    Date = new DateTime(2000, 1, 1),
                    Items = new[]
                    {
                        new TestMessageItem
                        {
                            Value = "different"
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
            public DateTime Date2 { get; set; }
            public string Value { get; set; }
            public Guid AGuid { get; set; }
            public Guid AGuid2 { get; set; }
            public TestMessageItem[] Items { get; set; }
        }

        private class TestMessageItem
        {
            public string Value { get; set; }
        }
    }
}