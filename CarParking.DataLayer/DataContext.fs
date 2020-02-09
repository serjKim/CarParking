namespace CarParking.DataLayer

module DataContext =
    open System.Data
    
    type ISQLServerDataContext =
        abstract member Connection : IDbConnection with get

    let inline getConn (dctx: ISQLServerDataContext) = dctx.Connection