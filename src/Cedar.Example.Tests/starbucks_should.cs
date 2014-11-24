namespace Cedar.Example.Tests
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Cedar.ProcessManagers;
    using Cedar.Testing;
    using ScenarioAttribute = Cedar.Testing.Xunit.ScenarioAttribute;

    public class starbucks_should
    {
        private readonly Guid _customerId = Guid.NewGuid();
        private readonly Guid _orderId = Guid.NewGuid();

        public class DrinkOrderPlaced
        {
            public Guid CustomerId { get; set; }
            public Guid OrderId { get; set; }
            public DrinkType Drink { get; set; }

            public override string ToString()
            {
                return "Ordered a " + Drink;
            }
        }

        public class DrinkPrepared
        {
            public Guid OrderId { get; set; }
            public DrinkType Drink { get; set; }
            public override string ToString()
            {
                return "Prepared a " + Drink;
            }
        }

        public class PaymentReceived
        {
            public Guid OrderId { get; set; }
            public decimal Amount { get; set; }

            public override string ToString()
            {
                return "Payment of " + Amount + " received";
            }
        }

        public class DrinkReceived
        {
            public Guid OrderId { get; set; }

            public override string ToString()
            {
                return "My god, it's full of stars!";
            }
        }

        public class PaymentRefunded
        {
            public Guid OrderId { get; set; }
            public decimal Amount { get; set; }

            public override string ToString()
            {
                return "Payment of " + Amount + " refunded";
            }
        }

        public class PrepareDrink
        {
            public DrinkType Drink { get; set; }
            public Guid OrderId { get; set; }

            public override string ToString()
            {
                return "Preparing a " + Drink;
            }
        }

        public class GiveCustomerDrink
        {
            public Guid CustomerId { get; set; }
            public Guid OrderId { get; set; }

            public override string ToString()
            {
                return "Order released.";
            }
        }

        public class RefundPayment
        {
            public Guid CustomerId { get; set; }
            public decimal Amount { get; set; }

            public override string ToString()
            {
                return "Refunding " + Amount;
            }
        }

        public enum DrinkType
        {
            Americano,
            Cappucino,
            Latte,
            Frappucino
        }

        public class StarbucksProcess : ObservableProcessManager
        {
            protected StarbucksProcess(string id, string correlationId)
                : base(id, correlationId)
            {
                var orderPlaced = OnEvent<DrinkOrderPlaced>();
                var drinkPrepared = OnEvent<DrinkPrepared>();
                var paymentReceived = OnEvent<PaymentReceived>();
                var drinkReceived = OnEvent<DrinkReceived>();
                var paymentRefunded = OnEvent<PaymentRefunded>();

                var orderReady = (from order in orderPlaced
                                  from drink in drinkPrepared
                                  from payment in paymentReceived
                                  select new
                                  {
                                      OrderedDrink = order.Drink,
                                      payment.Amount,
                                      order.CustomerId,
                                      PreparedDrink = drink.Drink,
                                      order.OrderId
                                  }).Distinct();

                var orderRuined = orderReady.Where(e => e.OrderedDrink != e.PreparedDrink);
                var orderCompleted = orderReady.Where(e => e.OrderedDrink == e.PreparedDrink);

                When(orderPlaced, e => new PrepareDrink { Drink = e.Drink, OrderId = e.OrderId });
                When(orderCompleted, e => new GiveCustomerDrink { OrderId = e.OrderId, CustomerId = e.CustomerId });
                When(orderRuined, e => new RefundPayment { CustomerId = e.CustomerId, Amount = e.Amount });

                CompleteWhen(drinkReceived);
                CompleteWhen(paymentRefunded);
            }
        }

        [Scenario]
        public async Task<ScenarioResult> prepare_a_drink_when_an_order_is_placed()
        {
            return await Scenario.ForProcess<StarbucksProcess>()
                .Given()
                .When(new DrinkOrderPlaced
                {
                    CustomerId = _customerId,
                    Drink = DrinkType.Latte,
                    OrderId = _orderId
                }).Then(new PrepareDrink {Drink = DrinkType.Latte, OrderId = _orderId});
        }

        [Scenario]
        public async Task<ScenarioResult> release_the_order_when_payment_is_received_and_drink_is_prepared()
        {
            return await Scenario.ForProcess<StarbucksProcess>()
                .Given(new DrinkOrderPlaced
                {
                    CustomerId = _customerId,
                    Drink = DrinkType.Latte,
                    OrderId = _orderId
                }, new DrinkPrepared {Drink = DrinkType.Latte, OrderId = _orderId})
                .When(new PaymentReceived {Amount = 10m, OrderId = _orderId})
                .Then(new GiveCustomerDrink {OrderId = _orderId, CustomerId = _customerId});
        }

        [Scenario]
        public async Task<ScenarioResult> not_release_the_order_when_payment_is_received_and_drink_is_not_prepared()
        {
            return await Scenario.ForProcess<StarbucksProcess>()
                .Given(new DrinkOrderPlaced
                {
                    CustomerId = _customerId,
                    Drink = DrinkType.Latte,
                    OrderId = _orderId
                })
                .When(new PaymentReceived {Amount = 10m, OrderId = _orderId})
                .ThenNothingWasSent();
        }

        [Scenario]
        public async Task<ScenarioResult> not_release_the_order_when_payment_is_not_received_and_drink_is_prepared()
        {
            return await Scenario.ForProcess<StarbucksProcess>()
                .Given(new DrinkOrderPlaced
                {
                    CustomerId = _customerId,
                    Drink = DrinkType.Latte,
                    OrderId = _orderId
                })
                .When(new DrinkPrepared { Drink = DrinkType.Latte, OrderId = _orderId })
                .ThenNothingWasSent();
        }

        [Scenario]
        public async Task<ScenarioResult> refund_the_payment_when_the_drink_is_ruined()
        {
            return await Scenario.ForProcess<StarbucksProcess>()
                .Given(new DrinkOrderPlaced
                {
                    CustomerId = _customerId,
                    Drink = DrinkType.Latte,
                    OrderId = _orderId
                }, new PaymentReceived {Amount = 10m, OrderId = _orderId})
                .When(new DrinkPrepared { Drink = DrinkType.Frappucino, OrderId = _orderId })
                .Then(new RefundPayment{Amount = 10m, CustomerId = _customerId});
        }

        [Scenario]
        public async Task<ScenarioResult> complete_the_process_when_the_customer_receives_drink()
        {
            return await Scenario.ForProcess<StarbucksProcess>()
                .Given(new DrinkOrderPlaced
                {
                    CustomerId = _customerId,
                    Drink = DrinkType.Latte,
                    OrderId = _orderId
                }, new PaymentReceived {Amount = 10m, OrderId = _orderId},
                    new DrinkPrepared {Drink = DrinkType.Latte, OrderId = _orderId})
                .When(new DrinkReceived {OrderId = _orderId})
                .ThenCompletes();
        }

        [Scenario]
        public async Task<ScenarioResult> complete_the_process_when_the_customer_receives_a_refund()
        {
            return await Scenario.ForProcess<StarbucksProcess>()
                .Given(new DrinkOrderPlaced
                {
                    CustomerId = _customerId,
                    Drink = DrinkType.Latte,
                    OrderId = _orderId
                }, new PaymentReceived { Amount = 10m, OrderId = _orderId },
                    new DrinkPrepared { Drink = DrinkType.Frappucino, OrderId = _orderId })
                .When(new PaymentRefunded { OrderId = _orderId, Amount = 10m})
                .ThenCompletes();
        }
    }
}