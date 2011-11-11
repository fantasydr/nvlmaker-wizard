using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Tjs
{
    #region Tjs数据类型
    public enum TjsType
    {
        Void,
        Number,
        String,
        Array,
        Dictionary,
    }

    // 基类
    public class TjsValue
    {
        public TjsType t;

        #region 缩进
        // 设置全局缩进单元
        public static string Indent
        {
            get { return _indent; }
            set { _indent = value; }
        }

        // 缩进单元
        protected static string _indent = " ";
        // 缩进堆栈
        protected static string _indentStack = string.Empty;
        #endregion
    }

    // 空值
    public class TjsVoid : TjsValue
    {
        public TjsVoid()
        {
            this.t = TjsType.Void;
        }

        public override string ToString()
        {
            return "void";
        }
    }

    // 字符串
    public class TjsString : TjsValue
    {
        public readonly string val;

        public TjsString(string val)
        {
            this.val = val;
            this.t = TjsType.String;
        }

        public override string ToString()
        {
            return string.Format("\"{0}\"", this.val);
        }
    }

    // 数字
    public class TjsNumber : TjsValue
    {
        public readonly double val;

        public TjsNumber(double val)
        {
            this.val = val;
            this.t = TjsType.Number;
        }

        public override string ToString()
        {
            return this.val.ToString();
        }
    }

    // 数组
    public class TjsArray : TjsValue
    {
        public readonly List<TjsValue> val;

        public TjsArray(List<TjsValue> val)
        {
            this.val = val;
            this.t = TjsType.Array;
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            // 保存当前缩进并增加缩进
            string savedIndent = _indentStack;
            if (_indent != null)
            {
                _indentStack += _indent;
            }

            // 当前默认缩进
            string currentIndent = _indentStack;

            buf.AppendLine("(const) [");
            int count = 0;
            foreach (TjsValue v in this.val)
            {
                buf.Append(currentIndent); buf.Append(v.ToString());

                // 末尾追加逗号分隔符
                if (++count < this.val.Count) buf.AppendLine(",");
            }
            buf.AppendLine(""); buf.Append(savedIndent); buf.Append("]");

            // 恢复缩进
            _indentStack = savedIndent;
            return buf.ToString();
        }
    }

    // 字典
    public class TjsDict : TjsValue
    {
        public readonly Dictionary<string, TjsValue> val;

        public TjsDict(Dictionary<string, TjsValue> val)
        {
            this.val = val;
            this.t = TjsType.Dictionary;
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            // 保存当前缩进并增加缩进
            string savedIndent = _indentStack;
            if (_indent != null)
            {
                _indentStack += _indent;
            }

            // 当前默认缩进
            string currentIndent = _indentStack;

            buf.AppendLine("(const) %[");
            int count = 0;
            foreach (KeyValuePair<string, TjsValue> kv in this.val)
            {
                buf.Append(currentIndent); buf.Append(kv.Key);
                buf.Append(" => "); buf.Append(kv.Value.ToString());

                // 末尾追加逗号分隔符
                if (++count < this.val.Count) buf.AppendLine(",");
            }
            buf.AppendLine(""); buf.Append(savedIndent); buf.Append("]");

            // 恢复缩进
            _indentStack = savedIndent;
            return buf.ToString();
        }
    }
    #endregion

    // Tjs数据解析器
    class TjsParser
    {
        // 以默认buffer大小初始化
        public TjsParser()
        {
            Reset(DEFAULT_BUFFER_SIZE);
        }

        // 以指定buffer大小初始化
        public TjsParser(int bufferSize)
        {
            Reset(bufferSize);
        }

        #region Tjs符号单元
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

            public TjsString ToTjsString()
            {
                if(this.val.Length >= 2)
                {
                    string inner = this.val.Substring(1, this.val.Length - 2);
                    TjsString val = new TjsString(inner);
                    return val;
                }
                return null;
            }

            public TjsNumber ToTjsNumber()
            {
                double inner = 0;
                if(double.TryParse(this.val, out inner))
                {
                    TjsNumber val = new TjsNumber(inner);
                    return val;
                }
                return null;
            }
        }
        #endregion

        #region 从数据流中解析Tjs符号
        Regex _regNumber = new Regex(@"[0-9\.]");
        Regex _regNonChar = new Regex(@"\s");
        Regex _regSeprater = new Regex(@"[,]");

        // 默认使用的buffer大小
        const int DEFAULT_BUFFER_SIZE = 8192;
        
        // 缓冲读取的文字流
        char[] _buffer;

        // 指向buffer中将要读取的字符
        int _pos;
        
        // 已解析的字符数
        int _parsed;

        // buffer中的实际有效长度
        int _len;

        // 重置所有变量
        void Reset(int size)
        {
            // 储存读取的文字流
            if (_buffer == null || _buffer.Length != size)
            {
                _buffer = new char[size];
            }

            // 指向buffer中将要读取的字符，初始状态假设Buffer已满
            _pos = size;

            // 已储存的字符数，配合_pos的初值
            _parsed = -size;

            // buffer中的实际有效长度
            _len = 0;

            // 重置错误信息
            _error = false;
        }

        // 已解析的字符数
        public int Parsed
        {
            get { return _parsed + _pos; }
        }

        // 读取文字流并填充buffer未满的部分
        void UpdateBuffer(TextReader r)
        {
            for (int i = _pos; i < _len; i++)
            {
                _buffer[i - _pos] = _buffer[i];
            }

            // 计算新的起始点
            int start = _len > _pos ? _len - _pos : 0;

            // 统计解析的字符数
            _parsed += _pos;

            // 重置当前位置
            _pos = 0;
            _len = start;

            // 如果还有则继续读取到buffer中
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
            int tail = -1; // 指向最后一个有效字符

            StringBuilder stored = new StringBuilder();

            while (_pos < _len)
            {
                // 读取一个字符，转成字串是为了匹配正则表达式
                string cur = _buffer[_pos++].ToString();

                //
                // 使用if-else是因为要用break来退出while循环
                //
                if(t == TokenType.Unknow)
                {
                    // 忽略这个无效字符
                    if (_regNonChar.IsMatch(cur)) { head++; }
                    // 发现字符串
                    else if (cur[0] == '"') { t = TokenType.String; }
                    // 发现数字
                    else if (_regNumber.IsMatch(cur)) { t = TokenType.Number; }
                    // 其余解释为符号
                    else { t = TokenType.Symbol; }
                }
                else if(t == TokenType.String)
                {
                    // 以此作为结尾
                    if (cur[0] == '"') { tail = _pos - 1; break; }
                }
                else if (t == TokenType.Number)
                {
                    // 忽略这个无效字符
                    if (_regNonChar.IsMatch(cur)) { tail = _pos - 2; break; }
                    // 保留这个字符
                    else if (!_regNumber.IsMatch(cur)) { _pos--; tail = _pos - 1; break; }
                }
                else if(t == TokenType.Symbol)
                {
                    // 忽略这个无效字符
                    if (_regNonChar.IsMatch(cur)) { tail = _pos - 2; break; }
                    // 保留这个字符
                    else if (_regSeprater.IsMatch(cur)) { _pos--; tail = _pos - 1; break; }
                }

                // 检查是否buffer已满
                if (_pos >= _buffer.Length)
                {
                    Debug.Assert(_pos == _buffer.Length, "_pos should not larger than buffer size");

                    if (_pos > head)
                    {
                        // 把buffer中未完的token进行储存
                        stored.Append(_buffer, head, _pos - head);
                    }

                    UpdateBuffer(r);
                    head = _pos;
                }
                // 读取结束
                else if (_pos >= _len)
                {
                    Debug.Assert(_pos == _len, "_pos should not larger than actual size");

                    tail = _pos - 1;
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

        #region 显示错误信息
        bool _error;
        public bool IsError
        {
            get { return _error; }
        }

        void ShowError(string msg)
        {
            _error = true;
            Console.Write("Error: ");
            Console.WriteLine(msg);
        }

        void ShowError(Token token)
        {
            _error = true;
            Console.Write("Error occurred at offset ");
            Console.Write(this.Parsed);
            Console.WriteLine(", parse terminated.");
            if(token == null)
            {
                Console.WriteLine("<< blank token >>");
            }
            else
            {
                Console.Write("Type=");
                Console.Write(token.t.ToString());
                Console.Write(", Value=");
                Console.WriteLine(token.val);
            }
        }
        #endregion

        #region 解析一个字典
        enum DictState
        {
            Key,
            Value,
        };
        TjsDict ParseDict(TextReader r)
        {
            TjsParser.Token token = null;

            Dictionary<string, TjsValue> inner = new Dictionary<string, TjsValue>();

            // 初始状态为读取key状态
            DictState s = DictState.Key;
            string key = null;

            do
            {
                token = GetNext(r);

                if(s == DictState.Key)
                {
                    // 读取键值
                    if(token.t == TokenType.String && key == null)
                    {
                        key = token.val;
                        
                        if(inner.ContainsKey(key))
                        {
                            // 重复的键值
                            ShowError("Duplicated Key");
                            break;
                        }

                        // 切换到读取Value状态
                        s = DictState.Value;
                    }
                    else
                    {
                        // 错误的键值
                        ShowError("Expect a Key");
                        break;
                    }
                }
                else if(s == DictState.Value)
                {
                    if (token.t == TokenType.Symbol)
                    {
                        if(key != null && token.val == "=>")
                        {
                            // 读取一个值
                            TjsValue val = Parse(r);

                            // 直接返回错误
                            if (val == null) return null;

                            inner.Add(key, val);
                            key = null;
                        }
                        else if(key == null && token.val == ",")
                        {
                            // 切换为读取key状态
                            s = DictState.Key;
                        }
                        else if(key == null && token.val == "]")
                        {
                            // 读取完毕
                            TjsDict ret = new TjsDict(inner);
                            return ret;
                        }
                        else
                        {
                            // 无效的符号
                            ShowError("Invalid Symbol");
                            break;
                        }
                    }
                    else
                    {
                        // 错误的符号
                        ShowError("Expect a Symbol");
                        break;
                    }
                }

            } while (token.t != TokenType.Unknow);

            ShowError("Dictionary Parsing Failed");
            ShowError(token);
            return null;
        }
        #endregion

        #region 解析一个数组
        TjsArray ParseArray(TextReader r)
        {
            TjsParser.Token token = null;

            List<TjsValue> inner = new List<TjsValue>();

            do
            {
                // 读取一个值
                TjsValue val = Parse(r);

                // 直接返回错误
                if (val == null) return null;

                inner.Add(val);

                // 读取其后的符号
                token = GetNext(r);
                if(token.t == TokenType.Symbol)
                {
                    if(token.val == "]")
                    {
                        // 读取完毕
                        TjsArray ret = new TjsArray(inner);
                        return ret;
                    }
                    else if(token.val != ",")
                    {
                        // 错误的符号
                        ShowError("Expect a Comma");
                        break;
                    }
                }
                else
                {
                    // 错误的符号
                    ShowError("Expect a Symbol");
                    break;
                }

            } while (token.t != TokenType.Unknow);

            ShowError("Array Parsing Failed");
            ShowError(token);
            return null;
        }
        #endregion

        #region 解析一个TjsValue
        public TjsValue Parse(TextReader r)
        {
            TjsParser.Token token = null;
            do
            {
                token = GetNext(r);
                if(token.t == TokenType.Number)
                {
                    // 解析出数字
                    TjsNumber ret = token.ToTjsNumber();
                    if (ret == null)
                    {
                        // 数字格式错误
                        ShowError("Invalid Number");
                        break;
                    }
                    return ret;
                }
                else if(token.t == TokenType.String)
                {
                    // 解析出字符串
                    TjsString ret = token.ToTjsString();
                    if (ret == null)
                    {
                        // 字符串格式错误
                        ShowError("Invalid String");
                        break;
                    }
                    return ret;
                }
                else if(token.t == TokenType.Symbol)
                {
                    if(token.val == "(const)")
                    {
                        // 啥也不干
                    }
                    else if(token.val == "void")
                    {
                        // 解析出空值
                        return new TjsVoid();
                    }
                    else if(token.val == "%[")
                    {
                        // 返回字典
                        TjsDict ret = ParseDict(r);
                        return ret;
                    }
                    else if(token.val == "[")
                    {
                        // 返回数组
                        TjsArray ret = ParseArray(r);
                        return ret;
                    }
                    else
                    {
                        // 无效的符号
                        ShowError("Invalid Symbol");
                        break;
                    }
                }
            } while (token.t != TokenType.Unknow);

            if(token.t != TokenType.Unknow)
            {
                ShowError("Value Parsing Failed");
                ShowError(token);
            }

            return null;
        }
        #endregion

    }
}
