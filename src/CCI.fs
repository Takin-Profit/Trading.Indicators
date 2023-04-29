module Incremental.Indicators.CCI

open System
open FSharp.Data.Adaptive
open Quotes
open Calc
open Util

type CciResult = { Value: float; Date: DateTime }



// cci calculation function
let calcCCI quotes (lookBack: int) =
    // convert Quotes to QuoteD
    let newQuotes = toQuoteDList quotes
    // create typical Prices list
    let typicalPrices =
        newQuotes |> AList.map (fun q -> (q.High + q.Low + q.Close) / 3.0)

    alist {
        let! len = AList.count newQuotes

        for i in 0..len do
            // check for enough data to calculate based on lookBack length
            if i + 1 >= lookBack then
                // offset to grab the current typicalPrice or Quote
                let current = i + 1
                // get the current typical price
                let! currentTP = getVal current 0.0 typicalPrices
                // gets the current Quote
                let! currentQuote = getVal current QuoteD.Empty newQuotes
                // position to start grabbing items from in the typicalPrices list
                let offset = i + 1 - lookBack
                // grab a chunk of data from the typicalPrices list to calculate the SMA with
                let period = AList.sub offset lookBack typicalPrices
                // sma of typical prices over given lookBack period
                let! typicalPriceSMA = AList.average period
                // mean deviation of the typical prices from the SMA
                let! deviationSMA = meanDev period

                // cci value calculation
                let cciValue =
                    if deviationSMA <> 0.0 then
                        let factor = double 0.015
                        let numerator = (currentTP - typicalPriceSMA)
                        let denominator = (factor * deviationSMA)
                        (numerator / denominator)
                    else
                        Double.NaN

                // return CciResult record as incremental value
                yield
                    { Value = cciValue
                      Date = currentQuote.Date }

    }



// let create a (quotes: Quote cset) =
//     if a < 1 then
//         Error("LookBack periods must be greater than 0 for Commodity Channel Index.")
//     else
//         let newQuotes = convert quotes
//         let typicalPrices = getTypicalPrices newQuotes
//         let sma = typicalPrices
