namespace CarParking.DataLayer

module DataContext =
    open System.Data
    
    type ICPDataContext =
        abstract member Connection : IDbConnection with get

    let inline getConn (dctx: ICPDataContext) = dctx.Connection

    type DataContext = ICPDataContext * System.Threading.CancellationToken