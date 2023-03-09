using System.Collections.Generic;

namespace PLC_Connection.InspectionResultDataModel
{
    /// <summary>
    ///  検査パーツごとに共通の処理をまとめたインターフェース
    /// </summary>
    interface Parts
    {
        /// <summary>
        ///  デバッグ用に表示する検査結果の文字列を作成する
        /// </summary>
        /// <returns> 結果を表す文字列。(部品名) : 結果 (○|×)の形 </returns>
        string GetResultString();

        /// <summary>
        ///  パーツごとに、エラー項目があるとそのエラーコードのリストを作成して返す。
        /// </summary>
        /// <returns> エラーコードのリスト。エラー項目がない場合はnull </returns>
        List<string> GetErrorCodes();
    }
}