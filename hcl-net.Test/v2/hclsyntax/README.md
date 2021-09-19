# Useful regex replacements for translating from GoLang test cases to C#

(For reference)

```
\{\s*(?:`([^`]*)`|"([^"]*)"),(.*)\n\s*\[\]Token\{\n

new TestCase\n\t\t\t\t\{\n\t\t\t\t\tInput = @"$1$2",$3\n\t\t\t\t\tExpectedTokens = new []\n\t\t\t\t\t\{\n

---

\{\n\s*Type:\s*(.*),\n\s*Bytes:\s*(.*),(.*)\n\s*Range:\s*hcl.Range\{(.*)\n\s*Start:\s*hcl.Pos\{Byte:\s*(\d+),\s*Line:\s*(\d+),\s*Column:\s*(\d+)},\n\s*End:\s*hcl.Pos\{Byte:\s*(\d+),\s*Line:\s*(\d+),\s*Column:\s*(\d+)},\n\s*},\n\s*\},

new ExpectedToken\n\t\t\t\t\t\t\{\n\t\t\t\t\t\t\tType = TokenType.$1,\n\t\t\t\t\t\t\tExpectedBytes = $2,$3\n\t\t\t\t\t\t\tExpectedRange = new ExpectedRange$4\n\t\t\t\t\t\t\t\{\n\t\t\t\t\t\t\t\tStart = \( Byte: $5, Line: $6, Column: $7 \),\n\t\t\t\t\t\t\t\tEnd = \( Byte: $8, Line: $9, Column: $10 \),\n\t\t\t\t\t\t\t\},\n\t\t\t\t\t\t\},
```