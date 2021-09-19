using System;
using System.Linq;
using System.Collections.Generic;

namespace hcl_net.v2.hclsyntax
{
	internal static partial class Scanner
	{
		#region State machine data
		static readonly sbyte  []_hclstrtok_actions = { 0, 1, 0, 1, 1, 2, 1, 0, 0,  };
		static readonly short  []_hclstrtok_key_offsets = { 0, 0, 2, 4, 6, 10, 14, 18, 22, 27, 31, 36, 41, 46, 51, 57, 62, 72, 83, 94, 105, 116, 127, 138, 149, 0,  };
		static readonly char  []_hclstrtok_trans_keys = { '\u0080', '\u00bf', '\u0080', '\u00bf', '\u0080', '\u00bf', '\u000a', '\u000d', '\u0024', '\u0025', '\u000a', '\u000d', '\u0024', '\u0025', '\u000a', '\u000d', '\u0024', '\u0025', '\u000a', '\u000d', '\u0024', '\u0025', '\u000a', '\u000d', '\u0024', '\u0025', '\u007b', '\u000a', '\u000d', '\u0024', '\u0025', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u007b', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u0055', '\u0075', '\u0000', '\u007f', '\u00c0', '\u00df', '\u00e0', '\u00ef', '\u00f0', '\u00f7', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000a', '\u000d', '\u0024', '\u0025', '\u005c', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u0000',  };
		static readonly sbyte  []_hclstrtok_single_lengths = { 0, 0, 0, 0, 4, 4, 4, 4, 5, 4, 5, 5, 5, 5, 6, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 0,  };
		static readonly sbyte  []_hclstrtok_range_lengths = { 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 3, 3, 3, 3, 3, 3, 3, 3, 0,  };
		static readonly short  []_hclstrtok_index_offsets = { 0, 0, 2, 4, 6, 11, 16, 21, 26, 32, 37, 43, 49, 55, 61, 68, 74, 81, 90, 99, 108, 117, 126, 135, 144, 0,  };
		static readonly sbyte  []_hclstrtok_cond_targs = { 11, 0, 1, 0, 2, 0, 5, 6, 7, 9, 4, 5, 6, 7, 9, 4, 5, 6, 7, 9, 4, 5, 6, 8, 9, 4, 5, 6, 7, 9, 5, 4, 5, 6, 7, 8, 4, 11, 12, 13, 15, 16, 10, 11, 12, 13, 15, 16, 10, 11, 12, 13, 15, 16, 10, 11, 12, 14, 15, 16, 10, 11, 12, 13, 15, 16, 11, 10, 11, 12, 13, 14, 16, 10, 17, 21, 11, 1, 2, 3, 10, 11, 12, 13, 15, 16, 18, 18, 18, 10, 11, 12, 13, 15, 16, 19, 19, 19, 10, 11, 12, 13, 15, 16, 20, 20, 20, 10, 11, 12, 13, 15, 16, 21, 21, 21, 10, 11, 12, 13, 15, 16, 22, 22, 22, 10, 11, 12, 13, 15, 16, 23, 23, 23, 10, 11, 12, 13, 15, 16, 24, 24, 24, 10, 11, 12, 13, 15, 16, 11, 11, 11, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 0,  };
		static readonly sbyte  []_hclstrtok_cond_actions = { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 5, 5, 5, 5, 3, 0, 5, 5, 5, 3, 5, 5, 0, 5, 3, 5, 5, 5, 5, 0, 3, 5, 5, 5, 0, 3, 1, 1, 1, 1, 1, 0, 5, 5, 5, 5, 5, 3, 0, 5, 5, 5, 5, 3, 5, 5, 0, 5, 5, 3, 5, 5, 5, 5, 5, 0, 3, 5, 5, 5, 0, 5, 3, 0, 0, 0, 0, 0, 0, 3, 5, 5, 5, 5, 5, 0, 0, 0, 3, 5, 5, 5, 5, 5, 0, 0, 0, 3, 5, 5, 5, 5, 5, 0, 0, 0, 3, 5, 5, 5, 5, 5, 0, 0, 0, 3, 5, 5, 5, 5, 5, 0, 0, 0, 3, 5, 5, 5, 5, 5, 0, 0, 0, 3, 5, 5, 5, 5, 5, 0, 0, 0, 3, 5, 5, 5, 5, 5, 0, 0, 0, 3, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0,  };
		static readonly short  []_hclstrtok_eof_trans = { 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 0,  };
		static readonly int  hclstrtok_start  = 4;
		static readonly int  hclstrtok_first_final  = 4;
		static readonly int  hclstrtok_error  = 0;
		static readonly int  hclstrtok_en_quoted  = 10;
		static readonly int  hclstrtok_en_unquoted  = 4;
		#endregion
		
		public static IEnumerable<byte[]> ScanStringLiteral(byte[] data, bool quoted)
		{
			var p = 0;  // "Pointer" into data
			var pe = data.Length; // End-of-data "pointer"
			var ts = 0;
			var te = 0;
			var eof = pe;
			
			// current state
			var cs = quoted ? hclstrtok_en_quoted : hclstrtok_en_unquoted;
			
			
			{
			}
			
			{
				int _klen;
				uint _trans = 0;
				int _keys;int _acts;uint _nacts;
				_resume: {}
				if ( p == pe && p != eof  )
				goto _out;
				
				if ( p == eof  )
				{
					if ( _hclstrtok_eof_trans[cs] > 0 )
					{
						_trans = (uint )_hclstrtok_eof_trans[cs] - 1;
					}
					
				}
				
				else
				{
					_keys = _hclstrtok_key_offsets[cs] ;
					_trans = (uint )_hclstrtok_index_offsets[cs];
					_klen = (int)_hclstrtok_single_lengths[cs];
					if ( _klen > 0 )
					{
						int _lower = _keys;int _upper = _keys + _klen - 1;int _mid;while ( true  )
						{
							if ( _upper < _lower  )
							{
								_keys += _klen;
								_trans += (uint )_klen;
								break;
							}
							
							
							_mid = _lower + ((_upper-_lower) >> 1);
							if ( ( data[p ]) < _hclstrtok_trans_keys[_mid ] )
							_upper = _mid - 1;
							
							else if ( ( data[p ]) > _hclstrtok_trans_keys[_mid ] )
							_lower = _mid + 1;
							
							else
							{
								_trans += (uint )(_mid - _keys);
								goto _match;
							}
							
						}
						
					}
					
					
					_klen = (int)_hclstrtok_range_lengths[cs];
					if ( _klen > 0 )
					{
						int _lower = _keys;int _upper = _keys + (_klen<<1) - 2;int _mid;while ( true  )
						{
							if ( _upper < _lower  )
							{
								_trans += (uint )_klen;
								break;
							}
							
							
							_mid = _lower + (((_upper-_lower) >> 1) & ~1);
							if ( ( data[p ]) < _hclstrtok_trans_keys[_mid ] )
							_upper = _mid - 2;
							
							else if ( ( data[p ]) > _hclstrtok_trans_keys[_mid + 1] )
							_lower = _mid + 2;
							
							else
							{
								_trans += (uint )((_mid - _keys)>>1);
								break;
							}
							
						}
						
					}
					
					
					_match: {}
				}
				
				cs = (int)_hclstrtok_cond_targs[_trans];
				if ( _hclstrtok_cond_actions[_trans] != 0 )
				{
				
					_acts = _hclstrtok_cond_actions[_trans] ;
					_nacts = (uint )_hclstrtok_actions[_acts ];
					_acts += 1;
					while ( _nacts > 0 )
					{
						switch ( _hclstrtok_actions[_acts ] ) {
							case 0:
							{if (te < p) {
									yield return data[te..p];
								}
								ts = p;
							}
							
							break;
							case 1:
							{te = p;
								yield return data[ts..te];
							}
							
							break;
							
						}
						_nacts -= 1;
						_acts += 1;
					}
					
					
				}
				
				
				if ( p == eof  )
				{
					if ( cs >= 4 )
					goto _out;
					
				}
				
				else
				{
					if ( cs != 0 )
					{
						p += 1;
						goto _resume;
					}
					
				}
				
				_out: {}
			}
			if (te < p) {
				// Collect any leftover literal characters at the end of the input
				yield return data[te..p];
			}
			
			// If we fall out here without being in a final state then we've
			// encountered something that the scanner can't match, which should
			// be impossible (the scanner matches all bytes _somehow_) but we'll
			// tolerate it and let the caller deal with it.
			if (cs < hclstrtok_first_final) {
				yield return data[p..data.Length];
			}
			
		}
	}
}
