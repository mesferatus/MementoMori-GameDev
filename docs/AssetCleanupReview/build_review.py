"""Read-only source audit; writes only review artifacts in docs/."""
from __future__ import annotations

from collections import Counter, defaultdict
from datetime import datetime
from hashlib import sha256
from pathlib import Path
import csv
import re

from PIL import Image, ImageDraw, ImageFont, ImageOps

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "docs" / "AssetCleanupReview"
REPORT = ROOT / "docs" / "ASSET_CLEANUP_REVIEW_2026-09-25.md"
IMAGE_EXT = {".png", ".psd", ".jpg", ".jpeg", ".tga", ".tif", ".tiff"}
REF_EXT = {".unity", ".prefab", ".anim", ".controller", ".asset", ".mat", ".overridecontroller", ".playable", ".spriteatlas", ".cs"}
GUID_RE = re.compile(r"^guid:\s*([0-9a-f]{32})", re.M)

def rel(path):
    return path.relative_to(ROOT).as_posix()

def group(path):
    p = rel(path).lower()
    if p.startswith("artstaging/"): return "Sources_Staging", "S"
    if "portrait" in p: return "Portraits", "PT"
    if "/art/ui/" in p or "/u1_" in p or "/u2_" in p or "/u3_" in p: return "UI", "UI"
    if "/sprites/vfx/" in p: return "VFX", "FX"
    if "melantha" in p or "m1_" in p or "m2_" in p: return "Melantha", "M"
    if "poe" in p or "p1_" in p or "p2_" in p: return "Poe", "P"
    if "andrealphus" in p or "a1_" in p or "a2_" in p: return "Andrealphus", "A"
    if "quarto" in p or "room" in p or "/q1_" in p or "/q2_" in p or "/q3_" in p or "/q4_" in p: return "Quarto", "Q"
    if "labirinto" in p or "labyrinth" in p or "/l1_" in p or "/l3_" in p: return "Labirinto", "LAB"
    if "dominiodalua" in p or "moon" in p or "lunar" in p or "mirror" in p or "/d1_" in p or "/d2_" in p or "/d3_" in p or "/d4_" in p or "/d5_" in p: return "DominioLua", "DL"
    if "_ui" in p or "logo" in p: return "UI", "UI"
    if "vfx" in p or "/fx" in p: return "VFX", "FX"
    return "Sources_Staging", "S"

def label(path, used, duplicate):
    p = rel(path).lower()
    source = p.startswith("artstaging/") or "_source" in p or ("/sheets/" in p and "_runtime" not in p and "_pixel" not in p and not p.endswith("u1_logo.png") and not p.endswith("u2_dialogueui.png") and not p.endswith("u3_pauseendui.png"))
    if source: return "SOURCE_ONLY", "SOURCE_OPTIONAL", "Arquivo de produção; decidir se a fonte deve ser preservada para edição futura."
    if duplicate and not used: return "DUPLICATE", "UNKNOWN_DEPENDENCY", "Mesmo conteúdo binário de outro arquivo; confirmar uso futuro e aprovar o ID específico."
    if "_runtime" in p or ("/tiles/" in p and used): return "RUNTIME_REQUIRED", "DO_NOT_DELETE", "Mantê-lo enquanto cena, tile ou clip depender dele."
    if used and ("/characters/melantha/" in p or "/sprites/" in p): return "OLD_BUT_REFERENCED", "DELETE_AFTER_RELINK", "Comparar com a arte aprovada; religar todas as referências antes de excluir."
    if used and ("/characters/poe/" in p or "/characters/andrealphus/" in p): return "RUNTIME_REQUIRED", "DO_NOT_DELETE", "Arte atual aprovada e ligada a clips; manter até decisão visual sobre substituto."
    if used: return "NEW_CANONICAL" if "/r3/" in p or "/r2/" in p else "RUNTIME_REQUIRED", "DO_NOT_DELETE", "Ligado ao jogo; preservar até uma substituição aprovada e testada."
    return "UNKNOWN_REVIEW", "UNKNOWN_DEPENDENCY", "Escolha visual e dependência indireta ainda pendentes."

def main():
    images = sorted(p for base in (ROOT / "Assets", ROOT / "ArtStaging") if base.exists() for p in base.rglob("*") if p.is_file() and p.suffix.lower() in IMAGE_EXT)
    refs = defaultdict(list)
    for p in (ROOT / "Assets").rglob("*"):
        if not p.is_file() or p.suffix.lower() not in REF_EXT: continue
        try: body = p.read_text(encoding="utf-8", errors="ignore")
        except OSError: continue
        for guid in set(re.findall(r"\b[0-9a-f]{32}\b", body)):
            refs[guid].append(rel(p))
    hashes = defaultdict(list)
    for p in images:
        hashes[sha256(p.read_bytes()).hexdigest()].append(rel(p))
    rows, ids = [], Counter()
    for p in images:
        g, prefix = group(p)
        ids[prefix] += 1
        iid = f"{prefix}{ids[prefix]:03d}"
        meta = p.with_name(p.name + ".meta")
        match = GUID_RE.search(meta.read_text(encoding="utf-8", errors="ignore")) if meta.exists() else None
        guid = match.group(1) if match else ""
        direct = sorted(refs.get(guid, []))
        copies = hashes[sha256(p.read_bytes()).hexdigest()]
        cls, risk, rec = label(p, bool(direct), len(copies)>1)
        try:
            with Image.open(p) as im: size = f"{im.width}×{im.height}"
        except Exception: size = "não lida"
        name = p.name.lower()
        version = "R4" if "r4" in rel(p).lower() else "R3" if "/r3/" in rel(p).lower() else "R2" if "/r2/" in rel(p).lower() else "runtime" if "_runtime" in name else "fonte" if cls=="SOURCE_ONLY" else "anterior/sem rodada"
        rows.append(dict(ID=iid, PATH=rel(p), GROUP=g, VERSION=version, SIZE=size, MODIFIED=datetime.fromtimestamp(p.stat().st_mtime).strftime("%Y-%m-%d"), GUID=guid, USED_NOW="GUID direto" if direct else "não confirmado", REFERENCES="; ".join(direct), REF_COUNT=len(direct), CLASSIFICATION=cls, DELETE_RISK=risk, RECOMMENDATION=rec, COPIES="; ".join(copies) if len(copies)>1 else ""))
    names = ["Melantha", "Poe", "Andrealphus", "Portraits", "Quarto", "Labirinto", "DominioLua", "UI", "VFX", "Sources_Staging"]
    fontpath = Path("C:/Windows/Fonts/arial.ttf")
    font = ImageFont.truetype(str(fontpath), 17) if fontpath.exists() else ImageFont.load_default()
    small = ImageFont.truetype(str(fontpath), 13) if fontpath.exists() else ImageFont.load_default()
    sheets = defaultdict(list)
    for group_name in names:
        subset = [r for r in rows if r["GROUP"]==group_name]
        for page_index, start in enumerate(range(0,len(subset),30),1):
            part=subset[start:start+30]
            w,h=5*270,((len(part)+4)//5)*248+64
            canvas=Image.new("RGB",(w,h),(25,27,34)); d=ImageDraw.Draw(canvas)
            d.text((15,12),f"{group_name}  •  página {page_index}  •  {len(subset)} arquivos",font=font,fill="white")
            for j,r in enumerate(part):
                x=(j%5)*270; y=(j//5)*248+55
                d.rectangle((x+5,y+4,x+265,y+244),fill=(42,44,53),outline=(95,98,110))
                p=ROOT/r["PATH"]
                try:
                    with Image.open(p) as im:
                        im=im.convert("RGBA")
                        bg=Image.new("RGBA",im.size,(65,67,76,255)); bg.alpha_composite(im)
                        thumb=ImageOps.contain(bg.convert("RGB"),(248,174),Image.Resampling.NEAREST)
                        canvas.paste(thumb,(x+135-thumb.width//2,y+12+(174-thumb.height)//2))
                except Exception: d.text((x+20,y+60),"Preview indisponível",font=small,fill="white")
                d.text((x+12,y+190),r["ID"]+"  "+r["SIZE"],font=small,fill=(255,230,159))
                name=p.name
                d.text((x+12,y+210),name[:31],font=small,fill="white")
                d.text((x+12,y+227),r["VERSION"]+" · "+("ref. direta" if r["REF_COUNT"] else "sem ref. direta"),font=small,fill=(180,200,225))
            filename=f"{names.index(group_name)+1:02d}_{group_name}_{page_index:02d}.png"
            canvas.save(OUT/filename,optimize=True)
            sheets[group_name].append(filename)
    # Portrait sheet has nine imported subimages; show each expression separately for the decision.
    portrait = next((ROOT/r["PATH"] for r in rows if r["PATH"].endswith("C1_Portraits_Pixel.png")),None)
    if portrait:
        with Image.open(portrait) as im:
            if im.size==(1254,1254):
                board=Image.new("RGB",(3*440,3*470),(25,27,34)); d=ImageDraw.Draw(board)
                for i in range(9):
                    x=(i%3)*440; y=(i//3)*470
                    crop=im.crop(((i%3)*418,(i//3)*418,(i%3+1)*418,(i//3+1)*418)).convert("RGBA")
                    bg=Image.new("RGBA",crop.size,(65,67,76,255));bg.alpha_composite(crop)
                    board.paste(bg.convert("RGB"),(x+11,y+8))
                    d.text((x+12,y+433),f"C1 fatia {i+1:02d} · 418×418",font=font,fill="white")
                board.save(OUT/"04_Portraits_C1_Expressoes.png",optimize=True)
                sheets["Portraits"].append("04_Portraits_C1_Expressoes.png")
    with (OUT/"asset_index.csv").open("w",encoding="utf-8-sig",newline="") as f:
        writer=csv.DictWriter(f,fieldnames=rows[0].keys());writer.writeheader();writer.writerows(rows)
    counts=Counter(r["CLASSIFICATION"] for r in rows)
    lines=["# Auditoria visual para limpeza — 25/09/2026", "", "**Estado:** proposta para decisão da Callisto. Nenhum asset existente foi excluído, movido, renomeado ou alterado. As classificações abaixo são triagem, não aprovação artística.", "", f"**Escopo:** {len(rows)} arquivos de imagem em `Assets` e `ArtStaging`; {sum(r['REF_COUNT']>0 for r in rows)} com referência direta por GUID em arquivos textuais do projeto. Índice integral: [asset_index.csv](AssetCleanupReview/asset_index.csv).", "", "A data é apenas contexto. `USED_NOW = GUID direto` significa vínculo em arquivo, não comprova renderização em Play Mode. Ausência de GUID não exclui carga por caminho, subasset, Addressables ou uso editorial. Não houve teste de Editor/Play Mode nesta auditoria de leitura.", "", "## Resumo de classificações", "", "| Categoria | Arquivos |", "|---|---:|"]
    for k in ("NEW_CANONICAL","NEW_UNUSED_BUT_GOOD","OLD_REJECTED","OLD_BUT_REFERENCED","DUPLICATE","SOURCE_ONLY","RUNTIME_REQUIRED","BAD_SLICE","BAD_IMPORT","UNKNOWN_REVIEW","SAFE_TO_DELETE_AFTER_APPROVAL"):
        lines.append(f"| {k} | {counts[k]} |")
    lines += ["", "`OLD_REJECTED` e `NEW_UNUSED_BUT_GOOD` exigem escolha visual humana; não foram inferidos por data ou por falta de referência. `SAFE_TO_DELETE_AFTER_APPROVAL` permanece vazio até essa escolha e a verificação de dependências indiretas.", "", "## Pranchas de revisão", ""]
    for n in names:
        lines += [f"### {n}", ""]
        for sheet in sheets[n]: lines.append(f"![{n} — {sheet}](AssetCleanupReview/{sheet})")
        lines.append("")
        if n=="Portraits":
            lines += ["`PT001–PT002`: retratos pequenos de objeto do Quarto (32×40). `PT003`: C1 pixel em uso; suas nove fatias ampliadas mostram Melantha (01–03), Andrealphus (04–06) e Poe (07–09). `PT004`: C1 Source com desenho distinto, sem vínculo direto; `PT005`: moldura, não expressão. A escolha entre os conjuntos e expressões pertence à Callisto.", ""]
    lines += ["## Índice por ID", "", "Cada ID corresponde a um arquivo físico. Coluna `REFERENCES` lista todos os vínculos diretos no CSV; aqui aparecem até três caminhos por item para manter o relatório legível. Arquivos fonte não são equivalentes funcionais das sheets runtime.", "", "| ID | Preview | Path | Group | Version | Resolução · data | Used now | References | Classification | Delete risk | Recommendation |", "|---|---|---|---|---|---|---|---|---|---|---|"]
    for r in rows:
        sheet=next(s for s in sheets[r["GROUP"]] if int(s.rsplit("_",1)[1].split(".")[0])==(int(r["ID"].lstrip("ABCDEFGHIJKLMNOPQRSTUVWXYZ"))-1)//30+1)
        paths=r["REFERENCES"].split("; ") if r["REFERENCES"] else []
        rs="<br>".join(paths[:3])+(f"<br>+{len(paths)-3} no CSV" if len(paths)>3 else "") if paths else "—"
        lines.append(f"| {r['ID']} | [prancha](AssetCleanupReview/{sheet}) | `{r['PATH']}` | {r['GROUP']} | {r['VERSION']} | {r['SIZE']} · {r['MODIFIED']} | {r['USED_NOW']} | {rs} | {r['CLASSIFICATION']} | {r['DELETE_RISK']} | {r['RECOMMENDATION']} |")
    lines += ["", "## Dependências e substituições a planejar", "", "- **Melantha:** locomoção R3 consta como integrada em `docs/VISUAL_FINISH_R4_2026-09-25.md`; ações/cutscenes de `Characters/Melantha` ainda alimentam clips. Cada ID `OLD_BUT_REFERENCED` exige sprite novo correspondente, religação do clip e comparação de escala/expressão antes de exclusão.", "- **Poe:** a sheet runtime de locomoção e os quadros de ação antigos cumprem papéis distintos. A arte atual aprovada não autoriza remover quadros de ação ainda ligados aos clips; localizar substitutos visuais primeiro.", "- **Andrealphus:** os quadros em `Characters/Andrealphus` ainda sustentam os clips, conforme referência direta. Tratar como arte atual aprovada até seleção visual contrária.", "- **Tiles e UI:** sprites antigos conectados em cenas, prefabs e tiles precisam ser substituídos no objeto concreto, com teste visual posterior. Uma referência antiga não é razão suficiente para aprovação permanente.", "- **Sources:** `ArtStaging` não entra no build Unity; `SOURCE_OPTIONAL` significa preservar para possível edição futura, mesmo quando equivalente promovido existe em `Assets`.", "- **Código/loads:** a busca por `Resources.Load<Sprite/Texture>` e `AssetReference` em `Assets/Scripts` não encontrou cargas de imagem, mas GUID sem referência ainda não é prova de segurança. O CSV guarda os GUIDs para a etapa de religação.", "- **Duplicata exata:** `UI027` tem SHA-256 idêntico ao emblema do menu. Os papéis visuais são distintos; a exclusão ainda depende de decisão da Callisto.", "", "## Lista final para aprovação", "", "### MANTER", "", "- Assets `RUNTIME_REQUIRED` e `NEW_CANONICAL` enquanto as dependências atuais persistirem; a escolha visual final de retratos e UI permanece com a Callisto.", "", "### PROVAVELMENTE APAGAR", "", "- Nenhum ID autorizado por esta auditoria. Revisar visualmente `DUPLICATE` e `UNKNOWN_REVIEW` nas pranchas, depois decidir IDs exatos.", "", "### APAGAR APÓS RELIGAR REFERÊNCIAS", "", "- IDs `OLD_BUT_REFERENCED` apenas após mapear substituto por clip/cena/prefab/tile, religar, validar e obter aprovação explícita.", "", "### PRECISA DECISÃO DA CALLISTO", "", "- Todos os portraits, variantes antigas sem ligação direta, fontes opcionais e o par de emblemas idênticos; indicar IDs das pranchas.", "", "### NÃO APAGAR", "", "- Todos os arquivos nesta etapa. Em especial `.meta`/GUIDs e itens `DO_NOT_DELETE`.", ""]
    REPORT.write_text("\n".join(lines),encoding="utf-8")
    print(f"images={len(rows)} refs={sum(r['REF_COUNT']>0 for r in rows)} sheets={sum(map(len,sheets.values()))} counts={dict(counts)}")

if __name__=="__main__": main()
