using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LocalFictionReader.Models;
using Windows.Storage;

namespace LocalFictionReader.Services
{
    public class BookParserService
    {
        // 支持「第123章」「第一百二十三章」「第 12 回」「第三卷」等写法（阿拉伯数字与中文数字均可）
        private readonly Regex _chapterRegex = new Regex(@"第\s*[0-9一二三四五六七八九十百千零两]+\s*[章节回卷]", RegexOptions.Compiled);

        public async Task<Book> ParseBookAsync(StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            // 通过 StorageFile 读取，避免直接使用 File.* 访问 picker 返回路径时抛 UnauthorizedAccessException
            IBuffer buffer = await FileIO.ReadBufferAsync(file);
            string content = DecodeText(buffer.ToArray());

            var book = new Book
            {
                Title = string.IsNullOrWhiteSpace(file.DisplayName) ? file.Name : file.DisplayName,
                FilePath = file.Path
            };

            MatchCollection matches = _chapterRegex.Matches(content);
            if (matches.Count == 0)
            {
                // 一个章节都没匹配到（例如无章节标题的短篇）时，整本书作为正文，避免页面空白
                book.Chapters.Add(new Chapter
                {
                    Title = "正文",
                    Content = content,
                    Position = 0
                });
                return book;
            }

            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                int titleStart = GetLineStart(content, match.Index);
                int titleEnd = GetLineEnd(content, match.Index);

                if (titleStart < lastPos)
                {
                    titleStart = lastPos;
                }

                if (titleEnd < titleStart)
                {
                    titleEnd = titleStart;
                }

                if (titleStart > lastPos)
                {
                    string preface = content.Substring(lastPos, titleStart - lastPos).Trim();
                    if (preface.Length > 0)
                    {
                        book.Chapters.Add(new Chapter
                        {
                            Title = "前言",
                            Content = preface,
                            Position = book.Chapters.Count
                        });
                    }
                }

                int nextStart = i + 1 < matches.Count
                    ? GetLineStart(content, matches[i + 1].Index)
                    : content.Length;

                if (nextStart < titleEnd)
                {
                    nextStart = titleEnd;
                }

                book.Chapters.Add(new Chapter
                {
                    Title = content.Substring(titleStart, titleEnd - titleStart).Trim(),
                    Content = content.Substring(titleEnd, nextStart - titleEnd).Trim(),
                    Position = book.Chapters.Count
                });

                lastPos = nextStart;
            }

            return book;
        }

        /// <summary>
        /// 按 BOM → 严格 UTF-8 → GBK(936) 的顺序解码文本，解决中文 txt 乱码问题。
        /// </summary>
        private static string DecodeText(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return string.Empty;
            }

            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                return new UTF8Encoding(false).GetString(bytes, 3, bytes.Length - 3);
            }

            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
            }

            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
            }

            try
            {
                return new UTF8Encoding(false, true).GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                try
                {
                    // GBK / GB2312 系中文 txt
                    return Encoding.GetEncoding(936).GetString(bytes);
                }
                catch (Exception)
                {
                    // 个别运行环境不提供 936 代码页，退回到宽松 UTF-8，保证仍能显示内容
                    return Encoding.UTF8.GetString(bytes);
                }
            }
        }

        private static int GetLineStart(string text, int index)
        {
            if (index <= 0 || text.Length == 0)
            {
                return 0;
            }

            int searchFrom = Math.Min(index - 1, text.Length - 1);
            int newLine = text.LastIndexOf('\n', searchFrom);
            return newLine < 0 ? 0 : newLine + 1;
        }

        private static int GetLineEnd(string text, int index)
        {
            if (text.Length == 0)
            {
                return 0;
            }

            int newLine = text.IndexOf('\n', Math.Min(index, text.Length - 1));
            if (newLine < 0)
            {
                return text.Length;
            }

            if (newLine > 0 && text[newLine - 1] == '\r')
            {
                return newLine - 1;
            }

            return newLine;
        }
    }
}
