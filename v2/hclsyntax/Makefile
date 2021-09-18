define run_ragel
	docker run \
	  --rm -it \
	  -v $(PWD):/work \
	  jmarkp/ragel:7.0.4 \
	  /opt/colm.net/ragel/bin/ragel-csharp \
	  -T0 --no-fork --rlhc -s $(1) -o $(patsubst %.cs.rl,%.cs,$(1))
	rm $(patsubst /work/%.cs.rl,%.ri,$(1))
endef

all: scan_tokens.cs scan_string_lit.cs

%.cs: %.cs.rl
	$(call run_ragel,/work/$<)
