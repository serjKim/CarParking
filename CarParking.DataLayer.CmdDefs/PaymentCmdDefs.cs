namespace CarParking.DataLayer.CmdDefs
{
    using System.Data;
    using System.Threading;
    using Dapper;
    using static CarParking.DataLayer.Dto;

    public class PaymentCmdDefs
    {
        public static CommandDefinition InsertPayment(PaymentDto paymentDto, IDbTransaction tran, CancellationToken token = default)
        {
            var sqlQuery = $@"
                insert into dbo.Payment
                    (PaymentID,
                    CreateDate)
                values
                    (@{nameof(PaymentDto.PaymentId)},
                    @{nameof(PaymentDto.CreateDate)})
            ";

            return new CommandDefinition(sqlQuery, paymentDto, cancellationToken: token, transaction: tran);
        }
    }
}
