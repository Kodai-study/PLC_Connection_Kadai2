using PLC_Connection.InspectionResultDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection
{
    /// <summary>
    ///  PLCから読み取れる、いろいろなブロックのデータを処理するクラス
    /// </summary>
    /// <remarks>
    ///  PLCからのデータの読み込みは、16ビット単位で行う。
    ///  それらのデータの区切りと意味の区切りは一緒でないため、
    ///  いろいろな変換を行う必要がある。
    /// </remarks>
    public abstract class ResultDataCreater
    {
        /// <summary>
        ///  読み取ったブロック
        /// </summary>
        public abstract int MASK { get; }

        /// <summary>
        ///  読み取れたビットブロックから、検査結果を読み取る。
        /// </summary>
        /// <param name="checkResults"> 検査結果を格納するクラスのインスタンス。中身が書き直され、結果が格納される </param>
        /// <param name="bitData"> 読み込んだ16ビットのブロック </param>
        public abstract void CheckResult(ref Results checkResults, int bitData);
    }
}
