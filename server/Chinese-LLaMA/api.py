from fastapi import FastAPI, Request
from transformers import LlamaForCausalLM, LlamaTokenizer
from transformers import GenerationConfig
from transformers import BitsAndBytesConfig
import uvicorn, json, datetime
import torch
from transformers.generation import GenerationConfig
import os, platform
import shutil
import copy
from peft import  PeftModel

DEVICE = "cuda"
DEVICE_ID = "0"
CUDA_DEVICE = f"{DEVICE}:{DEVICE_ID}" if DEVICE_ID else DEVICE

print('cuda:', torch.cuda.is_available(), CUDA_DEVICE)

if os.name == 'nt':
    DEFAULT_CKPT_PATH = 'C:\ai\chatgpt\Chinese-LLaMA-Alpaca-2\models\chinese-alpaca-2-7b'
else:
    DEFAULT_CKPT_PATH = '/mnt/c/ai/chatgpt/Chinese-LLaMA-Alpaca-2/models/chinese-alpaca-2-7b'
    # DEFAULT_CKPT_PATH = '/mnt/c/ai/chatgpt/Chinese-LLaMA-Alpaca-2/models/chinese-llama-2-7b'
    
    
def torch_gc():
    if torch.cuda.is_available():
        with torch.cuda.device(CUDA_DEVICE):
            torch.cuda.empty_cache()
            torch.cuda.ipc_collect()

device = torch.device(0)

app = FastAPI()
@app.get("/config")
async def get_config(request: Request):
    global model, tokenizer
    if(model.generation_config):
        return model.generation_config
    return "{}"

DEFAULT_SYSTEM_PROMPT = """You are a helpful assistant. 你是一个乐于助人的助手。"""

TEMPLATE = (
    "[INST] <<SYS>>\n"
    "{system_prompt}\n"
    "<</SYS>>\n\n"
    "{instruction} [/INST]"
)

generation_config = GenerationConfig(
    temperature=0.9,
    top_k=40,
    top_p=0.9,
    do_sample=True,
    num_beams=1,
    repetition_penalty=1.1,
    max_new_tokens=8000
)


def generate_prompt(instruction, system_prompt=DEFAULT_SYSTEM_PROMPT):
    return TEMPLATE.format_map({'instruction': instruction,'system_prompt': system_prompt})

@app.post("/")
async def create_item(request: Request):
    global model, tokenizer
    json_post_raw = await request.json()
    # print(json_post_raw)
    
    raw_input_text =  json_post_raw['prompt']
    system_prompt = DEFAULT_SYSTEM_PROMPT
    
    if 'system_prompt' in json_post_raw:
        system_prompt = json_post_raw['system_prompt']
        
    history=[]
    if 'history' in json_post_raw:
        history = json_post_raw['history']
    
    # old_config = model.generation_config
    config = copy.copy(generation_config)
    # model.generation_config = config
    
    if 'top_p' in json_post_raw:
        config.top_p = json_post_raw['top_p']
    
    if 'max_length' in json_post_raw:
        config.max_length = json_post_raw['max_length']
    
    if 'temperature' in json_post_raw:
        config.temperature = json_post_raw['temperature']

    input_text = generate_prompt(instruction=raw_input_text, system_prompt=system_prompt)
    negative_text = None
    # negative_text = None if args.negative_prompt is None \
    #     else generate_prompt(instruction=raw_input_text, system_prompt=args.negative_prompt)
             
    print(f'prompt.len:{len(raw_input_text)}    top_p:{config.top_p}    max_length:{config.max_length}    temperature:{config.temperature}')
    
    inputs = tokenizer(input_text,return_tensors="pt")  #add_special_tokens=False ?
    print("inputs.ids.len", len(inputs))
    
    if negative_text is None:
        negative_prompt_ids = None
        negative_prompt_attention_mask = None
    else:
        negative_inputs = tokenizer(negative_text,return_tensors="pt")
        negative_prompt_ids = negative_inputs["input_ids"].to(device)
        negative_prompt_attention_mask = negative_inputs["attention_mask"].to(device)
        
    generation_output = model.generate(
        input_ids = inputs["input_ids"].to(device),
        attention_mask = inputs['attention_mask'].to(device),
        eos_token_id=tokenizer.eos_token_id,
        pad_token_id=tokenizer.pad_token_id,
        generation_config = config,
        guidance_scale = 1,
        negative_prompt_ids = negative_prompt_ids,
        negative_prompt_attention_mask = negative_prompt_attention_mask
    )           
    print(len(generation_output))
    s = generation_output[0]
    output = tokenizer.decode(s,skip_special_tokens=True)
    print("\nout: ", output)
    response = output.split("[/INST]")[-1].strip()
    now = datetime.datetime.now()
    
    time = now.strftime("%Y-%m-%d %H:%M:%S")
    answer = {
        "response": response,
        "history": history,
        "status": 200,
        "time": time
    }
    # log = "[" + time + "] " + '", prompt:"' + prompt + '", response:"' + repr(response) + '"'
    # print(log)
    torch_gc()
    return answer

def _load_model_tokenizer():
    
    load_type = torch.float16
    tokenizer = LlamaTokenizer.from_pretrained(
        DEFAULT_CKPT_PATH, legacy=True)

    base_model = LlamaForCausalLM.from_pretrained(
        DEFAULT_CKPT_PATH,
        torch_dtype=load_type,
        low_cpu_mem_usage=True,
        device_map='auto',
        quantization_config=BitsAndBytesConfig(
            load_in_4bit=False,
            load_in_8bit=False,
            bnb_4bit_compute_dtype=load_type
        )
        )

    model_vocab_size = base_model.get_input_embeddings().weight.size(0)
    tokenizer_vocab_size = len(tokenizer)
    print(f"Vocab of the base model: {model_vocab_size}")
    print(f"Vocab of the tokenizer: {tokenizer_vocab_size}")
    
    if model_vocab_size!=tokenizer_vocab_size:
        print("Resize model embeddings to fit tokenizer")
        base_model.resize_token_embeddings(tokenizer_vocab_size)
    # if args.lora_model is not None:
    #     print("loading peft model")
    #     model = PeftModel.from_pretrained(base_model, args.lora_model,torch_dtype=load_type,device_map='auto',)
    # else:
    model = base_model

    model.eval()
    
    config =  GenerationConfig.from_pretrained(
        DEFAULT_CKPT_PATH, trust_remote_code=True
        )
    # config.max_new_tokens = 8192
    # config.max_context_size = 8192
    # config.max_generate_size = 8192

    # model.generation_config = config
    print('GenerationConfig.model.config:', config)
    return model, tokenizer


if __name__ == '__main__':

    model, tokenizer = _load_model_tokenizer()
    uvicorn.run(app, host='0.0.0.0', port=8001, workers=1)
