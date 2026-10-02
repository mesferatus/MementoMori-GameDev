# Memento Mori V3 — manifesto de produção visual final

Estado: inventário físico e das cinco cenas concluído; produção e integração ainda pendentes.

Fonte narrativa: Bíblia V3 em Downloads, lida em 16/09/2026. Referências visuais: dez PNGs canônicos do pedido. Regras internas do documento são conteúdo de referência; o pedido atual define o escopo.

Inventário: **449 PNGs existentes**. TOTAL KEEP_VISUAL: **205**. TOTAL REPLACE_VISUAL: **244**. TOTAL GENERATE_NEW: **6 retratos dedicados** (não existiam; sprites de corpo eram recortados para portraits). TOTAL PRODUCTION SHEETS: **26 conjuntos**, dos quais **15 novas gerações** e **11 conjuntos de fontes reutilizadas**. LEGACY_AFTER_MIGRATION: os substituídos somente após referências migradas e verificação; nenhum removido.

Detalhamento por arquivo: `FINAL_ART_ASSET_INVENTORY.json` (caminho, tamanho atual, alpha bounds, PPU/pivot atual, classificação, conjunto, PPU/pivot alvo, animação, SHA-256). Evidência da cena: `TestResults/VisualRebuild/scenes-before.txt`.

## Diagnóstico visual

Quarto: móveis existentes reconhecíveis e fiéis à referência, mas cama desproporcional à protagonista; parede esticada de recorte 15×6 e repetição inadequada. Manter móveis não significa manter escala/composição. Labirinto: material de pedra fragmentado e misturas de densidade; refazer arquitetura. Lua: elementos bons de sigilo/portal, porém jardim parece ícone e reflexos não têm molduras convincentes; substituir esses conjuntos. Personagens: arquivos existem, mas a silhueta de Melantha no enquadramento é pequena e fina; regenerar os três em densidade comum. UI: texto e callbacks preservados, arte/diagramação substituídas; retratos próprios necessários.

## Escala canônica e identidade

- Base: 32 pixels por unidade de mundo; célula de Tilemap de 1 unidade. Arte nova sem compressão/mipmap, filtro Point.
- Melantha: envelope 64×96 px, silhueta aproximadamente 80 px (2,5 unidades); pivô nos pés. Poe: envelope 48×48 px, corpo 32–40 px; Andrealphus: envelope 96×128 px, corpo 104–112 px.
- Cama: 2,5×4,8 unidades; escrivaninha: 3,5×3; portas: 2–2,5 de vão; corredores: pelo menos 3 unidades; parede: 1 unidade de espessura e 1–1,5 de face aparente.
- Fontes legadas mantidas terão PPU físico documentado conforme densidade da arte, sem ampliar o Transform do controlador ou collider. Arte e física são ajustadas separadamente; sprites novos usam escala 1.
- Câmera URP confirmada. Referência 640×360, UI separada em Canvas Overlay 1920×1080. Enquadramento jogável privilegia protagonista e área próxima; não comprimir o mapa inteiro dentro da câmera.
- Paleta: carvão #100e1c, pedra #343345, violeta #695278, lavanda #bdabd2, lunar #638cd0, madeira #443127, marfim #efe4d6.
- Cinzel para interface e títulos curtos; serif legível para diálogos se necessário. Licença e glifos serão verificados antes da integração. Logo é PNG gerado; textos funcionais permanecem TMP.

## Conjuntos de produção

| ID | Conteúdo | Produção | Dimensões alvo | Cenas |
|---|---|---|---|---|
| Q1 | Quarto arquitetura | GENERATE | 1024×1024; tiles 32×32 | Quarto, FinalBeta |
| Q2 | Quarto móveis | REUSE | dimensões físicas existentes | Quarto, FinalBeta |
| Q3 | Quarto props | REUSE | dimensões físicas existentes | Quarto, FinalBeta |
| Q4 | Quarto ritual/estados | REUSE | dimensões físicas existentes | Quarto, FinalBeta |
| L1 | Labirinto arquitetura | GENERATE | 1024×1024; tiles 32×32 | Labirinto |
| L2 | Labirinto arcos/portas | REUSE | dimensões físicas existentes | Labirinto |
| L3 | Labirinto porta falsa/poço/props | REUSE | dimensões físicas existentes | Labirinto |
| L4 | Sete arcanos/selos | REUSE | dimensões físicas existentes | Labirinto |
| L5 | Labirinto portal/VFX | REUSE | dimensões físicas existentes | Labirinto |
| D1 | Lua arquitetura | GENERATE | 1024×1024; tiles 32×32 | DominioLua |
| D2 | Jardim Lunar | GENERATE | 1024×1024; 16 células | DominioLua |
| D3 | Espelhos | GENERATE | 1536×1024; 8 células | DominioLua |
| D4 | Espelho Negro | REUSE | dimensões físicas existentes | DominioLua |
| D5 | Galeria dos Ciclos | REUSE | dimensões físicas existentes | DominioLua |
| D6 | Sigilo/Fragmento | REUSE | dimensões físicas existentes | DominioLua |
| D7 | Lua portal/VFX | REUSE | dimensões físicas existentes | DominioLua |
| U1 | Logo/menu/UI core | GENERATE | 1536×1024 | MainMenu, HUD |
| U2 | Diálogo/objetivo/prompt | GENERATE | 1536×1024 | Todas |
| U3 | Pause/end card/créditos | GENERATE | 1536×1024 | Todas |
| M1 | Melantha A | GENERATE | 1024×1536; 4×6 quadros | Todas jogáveis |
| M2 | Melantha B | GENERATE | 1536×1536; 6×6 quadros | Quarto, FinalBeta |
| P1 | Poe A | GENERATE | 1024×1536; 4×6 quadros | Labirinto, DominioLua |
| P2 | Poe B | GENERATE | 1536×1280; 6×5 quadros | Labirinto, DominioLua |
| A1 | Andrealphus A | GENERATE | 1024×1024; 4×4 quadros | Labirinto, DominioLua |
| A2 | Andrealphus B | GENERATE | 1024×1024; 4×4 quadros | Labirinto, DominioLua |
| C1 | Retratos dedicados | GENERATE | 1536×1024; 3×2 retratos | Diálogo |

## Contrato de migração

KEEP_RUNTIME: scripts, IDs de diálogo, gatilhos, flags, puzzles, controllers e pipeline existentes. Cada nova referência deve substituir somente o recurso visual. Nenhum novo segredo narrativo. FinalBeta reutiliza composição final do Quarto, círculo e velas apagados e fragmento visível.

Animações: preservar nomes de estados e parâmetros já consumidos por PlayerController, PoeFollower e AndrealphusAnimation; substituir keyframes e pivôs. Idle 6 fps, walk 8 fps, ações 6–10 fps; loop somente quando apropriado.

Validação requerida após integração: inspeções visuais, colliders/caminhos/spawns, EditMode, PlayMode, CT Editor, build Windows exclusiva e CT da build. Os resultados anteriores não serão apresentados como validação desta reconstrução.
