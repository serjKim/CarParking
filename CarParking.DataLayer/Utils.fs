namespace CarParking.DataLayer

open System.Runtime.CompilerServices
open System
open Dapper
open CarParking.Utils

[<Extension>]
type internal DynamicParametersExtensions() =
    [<Extension>]
    static member AddParam (this: DynamicParameters, name, value, ?dbType: Data.DbType) = 
        this.Add (name, value, dbType.ToNullable())
        this

