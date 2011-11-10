using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ResConverter
{
    class TjsParser
    {
        #region Tjs数据类型
        public enum TjsType
        {
            Number,
            String,
            Array,
            Dictionary,
        }

        public class TjsValue
        {
            public TjsType t;
        }

        public class TjsString : TjsValue
        {
            public string val;

            public TjsString(string val)
            {
                this.val = val;
                this.t = TjsType.String;
            }
        }

        public class TjsNumber : TjsValue
        {
            public double val;

            public TjsNumber(double val)
            {
                this.val = val;
                this.t = TjsType.Number;
            }
        }

        public class TjsArray : TjsValue
        {
            public List<TjsValue> val;

            public TjsArray(List<TjsValue> val)
            {
                this.val = val;
                this.t = TjsType.Array;
            }
        }

        public class TjsDict : TjsValue
        {
            public Dictionary<string, TjsValue> val;

            public TjsDict(Dictionary<string, TjsValue> val)
            {
                this.val = val;
                this.t = TjsType.Dictionary;
            }
        }
        #endregion

        #region 基本标记类型
        public enum TokenType
        {
            Unknow,
            String,
            Number,
            Symbol,
        }

        public class Token
        {
            public string val = string.Empty;
            public TokenType t = TokenType.Unknow;
        }
        #endregion

        #region 从数据流中解析Tjs符号
        Regex _regNumber = new Regex(@"[0-9\.]");
        Regex _regNonChar = new Regex(@"\s");

        const int BUFFER_SIZE = 8192;
        // 储存读取的文字流
        char[] _buffer = new char[BUFFER_SIZE];
        // 指向buffer中将要读取的字符，初始状态设定Buffer已满
        int _pos = BUFFER_SIZE;
        // buffer中的实际有效长度
        int _len = 0;

        // 读取文字流并填充buffer未满的部分
        void UpdateBuffer(TextReader r)
        {
            for (int i = _pos; i < _len; i++)
            {
                _buffer[i - _pos] = _buffer[i];
            }

            // 计算新的起始点
            int start = _len > _pos ? _len - _pos : 0;

            _pos = 0;
            _len = start;
            if (r.Peek() >= 0)
            {
                _len += r.ReadBlock(_buffer, start, _buffer.Length - start);
            }
        }

        public Token GetNext(TextReader r)
        {
            // 如果buffer已满则需要更新
            if (_pos >= _buffer.Length)
            {
                UpdateBuffer(r);
            }

            TokenType t = TokenType.Unknow;
            int head = _pos; // 指向第一个有效字符
            int tail = -1;  // 指向最后一个有效字符

            StringBuilder stored = new StringBuilder();

            while (_pos < _len)
            {
                char cur = _buffer[_pos++];

                //
                // 使用if-else是因为要用break来退出while循环
                //
                if(t == TokenType.Unknow)
                {
                    // 忽略这个无效字符
                    if (_regNonChar.IsMatch(cur.ToString())) { head++; }
                    // 发现字符串
                    else if (cur == '"') { t = TokenType.String; }
                    // 发现数字
                    else if (_regNumber.IsMatch(cur.ToString())) { t = TokenType.Number; }
                    // 其余解释为符号
                    else { t = TokenType.Symbol; }
                }
                else if(t == TokenType.String)
                {
                    // 以此作为结尾
                    if (cur == '"') { tail = _pos - 1; break; }
                }
                else if (t == TokenType.Number)
                {
                    // 忽略这个无效字符
                    if (_regNonChar.IsMatch(cur.ToString())) { tail = _pos - 2; break; }
                    // 保留这个字符
                    else if (!_regNumber.IsMatch(cur.ToString())) { _pos--; tail = _pos - 1; break; }
                }
                else if(t == TokenType.Symbol)
                {
                    // 忽略这个无效字符
                    if (_regNonChar.IsMatch(cur.ToString())) { tail = _pos - 2; break; }
                }

                // 检查是否buffer已满
                if (_pos >= _buffer.Length)
                {
                    if (_pos > head)
                    {
                        // 把buffer中未完的token进行储存
                        stored.Append(_buffer, head, _pos - head);
                    }

                    UpdateBuffer(r);
                    head = _pos;
                }
            }

            // 追加当前结果
            if (tail >= head)
            {
                stored.Append(_buffer, head, tail - head + 1);
            }

            // 返回最终值
            Token token = new Token();
            token.t = t;
            if (stored.Length > 0)
            {
                token.val = stored.ToString();
            }
            return token;
        }
        #endregion

        public TjsValue Parse(TextReader r)
        {
            return null;
        }
    }
}
