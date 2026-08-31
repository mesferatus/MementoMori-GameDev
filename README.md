# Memento Mori

Memento Mori é um jogo 2D narrativo de exploração e puzzles. A jornada acompanha Melantha por ambientes de fantasia gótica, com foco em diálogo, interação e descoberta.

## Fluxo jogável

`MainMenu → Quarto → Labirinto → DominioLua → FinalBeta`

- **MainMenu:** entrada e início de uma nova sessão.
- **Quarto:** apresentação, interações iniciais e ritual.
- **Labirinto:** travessia com portas, ecos, Poe e corredor ilusório.
- **DominioLua:** espelho, sigilo, fragmento e portal final.
- **FinalBeta:** encerramento da fatia vertical e retorno ao menu.

## Estrutura do projeto

- `Assets/` — código, cenas, prefabs, áudio, testes e recursos usados pelo jogo.
- `Assets/Scripts/` — scripts C# responsáveis pelos sistemas de jogo.
- `Assets/Scenes/` — cenas do fluxo principal.
- `Assets/LuizaAssets/` — materiais visuais usados na reconstrução das fases. Alguns packs ficam apenas no ambiente local por restrições de licença.
- `Packages/` — dependências do projeto Unity.
- `ProjectSettings/` — configurações do projeto Unity.
- `docs/` — arquitetura, roadmap, créditos, licenças e instruções de instalação dos assets locais.

Dentro de `Assets/`, arquivos `.cs` são códigos C#; `.unity` são cenas; `.prefab` são objetos reutilizáveis; `.asset` guarda configurações e dados do Unity; `.anim` e `.controller` tratam animações; `.asmdef` define assemblies; e `.meta` preserva os identificadores usados pelo Unity. Não remova ou mova arquivos `.meta` isoladamente.

## Como abrir

1. Instale o Unity `6000.4.12f1`.
2. Abra esta pasta pelo Unity Hub.
3. Aguarde a importação dos assets.
4. Abra `Assets/Scenes/MainMenu.unity` e use Play.

## CURRENT TECHNICAL BASELINE

Baseline pré-integração visual: Unity `6000.4.12f1`, build Windows x86_64, cinco cenas habilitadas na ordem do fluxo principal, Input System ativo, URP 17.4.0 e projeto identificado como `Memento Mori`.

Na validação mais recente, o projeto compilou, a build Windows foi gerada e as suítes automatizadas passaram: EditMode 24/24, PlayMode 11/11 e smoke CT-001 a CT-011. Missing References permaneceu em 0 nos scans disponíveis. O playthrough humano, vitória e New Game/reset foram confirmados anteriormente pelo usuário. Esses resultados cobrem a base técnica; não aprovam a integração visual final da Luiza.

Detalhes de release e QA estão em [docs/RELEASE_CHECKLIST.md](docs/RELEASE_CHECKLIST.md) e [docs/QA_PLAYTEST_TEMPLATE.md](docs/QA_PLAYTEST_TEMPLATE.md). A análise estatística está em [docs/STATISTICAL_QA_ANALYSIS.md](docs/STATISTICAL_QA_ANALYSIS.md).

## Equipe

- **Callisto:** programação, gameplay, integração, puzzles, áudio técnico, testes e build.
- **Luiza:** direção visual, tilemaps, sprites, props, composição das cenas, colliders de cenário e UI visual.
- **Ambas:** testes, Git, documentação e apresentação.

## Próximos passos

- Executar o playthrough visual/manual com a Luiza.
- Integrar ou revisar sprites, tilemaps, colliders, sorting e UI visuais.
- Revalidar a build após as alterações visuais e preparar a entrega acadêmica.

## Git

Use branches para mudanças isoladas e abra uma revisão antes de integrar alterações na principal. Arquivos gerados pelo Unity e packs locais restritos já são ignorados pelo Git.

## Créditos e licenças

Consulte [Assets e licenças](docs/ASSETS_AND_LICENSES.md). Para instalar os packs que não são versionados, siga [Configuração de assets](docs/ASSET_SETUP.md).
