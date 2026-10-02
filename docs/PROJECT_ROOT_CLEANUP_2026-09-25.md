# Auditoria da raiz — 25/09/2026

Escopo: itens imediatos da raiz, com soma recursiva dos arquivos (bytes decimais; tamanhos aproximados). Estado inspecionado com o projeto em desenvolvimento e três processos Unity abertos. Há alterações locais extensas em `Assets`, `ProjectSettings`, `Tools` e `docs`; nenhuma foi revertida. `develop/` não existe nesta raiz; há `devlog/`.

## Inventário completo, ordenado por tamanho

| Item | Tamanho | Tipo | Necessário | Regenerável | Risco | Ação |
|---|---:|---|---|---|---|---|
| `Library/` | 4.047 MB | GENERATED_CACHE | Não | Sim | Alto enquanto Unity usa | Apagar depois de fechar todas as instâncias Unity; reimportação será demorada |
| `Builds/` | 1.038 MB | GENERATED_BUILD / EVIDENCE_KEEP | Para entrega e teste | Sim, com custo | Alto | Manter build R4; revisar as duas anteriores |
| `.git/` | 272 MB | CORE_PROJECT | Sim | Não | Crítico | Manter |
| `Assets/` | 107 MB | CORE_PROJECT | Sim | Não | Crítico | Manter integralmente |
| `ArtStaging/` | 58,3 MB | PROJECT_CONTENT | Para produção de arte | Não plenamente | Alto | Manter; revisar fontes e backups separadamente |
| `.codex-checkpoints/` | 44,4 MB | EVIDENCE_OLD / REVIEW_REQUIRED | Incerto | Não | Médio | Revisar bundle único antes de remover |
| `TestResults/` | 24,4 MB | TEST_OUTPUT / EVIDENCE_KEEP | Parcialmente | Testes sim; evidência histórica não | Alto | Manter resultados citados; triagem de duplicatas |
| `Logs/` | 17,9 MB | TEST_OUTPUT / TEMPORARY | Parcialmente | Logs novos sim | Médio | Revisar logs de build/QA; não apagar com Unity aberto |
| `docs/` | 9,27 MB | PROJECT_CONTENT / EVIDENCE_KEEP | Sim | Não | Alto | Manter; classificar abaixo |
| `.agents/` | 1,91 MB | TOOLING_ACTIVE | Sim, no fluxo atual | Instalável, com custo | Médio | Manter skills locais |
| `Assembly-CSharp-Editor.csproj` | 102 KB | GENERATED_CACHE | IDE apenas | Sim | Baixo | Excluir quando Unity/IDE fechados, se desejado |
| `MementoMori.Runtime.csproj` | 83 KB | GENERATED_CACHE | IDE apenas | Sim | Baixo | Mesmo critério |
| `MementoMori.EditMode.Tests.csproj` | 76 KB | GENERATED_CACHE | IDE apenas | Sim | Baixo | Mesmo critério |
| `MementoMori.PlayMode.Tests.csproj` | 73 KB | GENERATED_CACHE | IDE apenas | Sim | Baixo | Mesmo critério |
| `ProjectSettings/` | 61,9 KB | CORE_PROJECT | Sim | Não | Crítico | Manter integralmente |
| `UserSettings/` | 58,1 KB | GENERATED_CACHE / OPTIONAL_DELETE | Não para compartilhamento | Sim | Baixo | Opcional após fechar Editor; contém preferências, busca e layouts locais |
| `Temp/` | 47,8 KB | GENERATED_CACHE | Não | Sim | Alto enquanto Unity usa | Apagar depois de fechar Unity; contém `UnityLockfile` |
| `Tools/` | 25,7 KB | TOOLING_ACTIVE / REVIEW_REQUIRED | Parcialmente | Não | Médio | Manter receitas R2–R4 e gerador narrativo; revisar após entrega |
| `Packages/` | 20,3 KB | CORE_PROJECT | Sim | Não | Crítico | Manter integralmente |
| `skills-lock.json` | 7,1 KB | TOOLING_ACTIVE | Para skills locais | Recriável, com custo | Baixo | Manter |
| `README.md` | 3,5 KB | PROJECT_CONTENT | Sim | Não | Baixo | Manter |
| `.vscode/` | 2,1 KB | TOOLING_ACTIVE | Útil para IDE | Manualmente | Baixo | Manter: settings, extensões e attach Unity |
| `devlog/` | 1,7 KB | PROJECT_CONTENT | Sim, exigido por CONTRIBUTING | Não | Baixo | Manter; `develop/` ausente |
| `AGENTS.md` | 790 B | TOOLING_ACTIVE | Sim, instruções do projeto | Não | Médio | Manter |
| `.gitignore` | 501 B | CORE_PROJECT | Sim | Não | Crítico | Manter |
| `CONTRIBUTING.md` | 427 B | PROJECT_CONTENT | Sim | Não | Baixo | Manter |
| `MementoMori-GameDev.slnx` | 241 B | GENERATED_CACHE | IDE apenas | Sim | Baixo | Excluir só com IDE/Unity fechados; `.vscode/settings.json` aponta para ele |
| `.gitattributes` | 206 B | CORE_PROJECT | Sim | Não | Crítico | Manter |
| `EvidenceDraft/` | 0 B | TEMPORARY / SAFE_TO_DELETE | Não; só continha `C5_FINAL/` vazia | Sim | Nenhum | Excluída nesta auditoria |
| `.c5-final-evidence/` | 0 B | TEMPORARY / SAFE_TO_DELETE | Não, vazia | Sim | Nenhum | Excluída nesta auditoria |
| `plastic4/` | 0 B | TOOLING_OLD / REVIEW_REQUIRED | Não aparente | Sim | Baixo | Revisar antes de excluir, conforme pedido |

Total aproximado antes da limpeza: **5,62 GB** (inclui `.git`). Tamanho é soma lógica dos arquivos, não espaço alocado nem uso adicional do OneDrive.

## Builds

| Conjunto | Tamanho | Estado | Decisão |
|---|---:|---|---|
| `Builds/MementoMori-validated.exe` + `_Data`, DLLs e runtime adjacentes | ~349 MB | LATEST_VALIDATED: build R4 de 25/09; `r4-build-summary.txt` informa sucesso, zero erros, 521 avisos | Manter; validação informada é de build, sem novo playtest nesta auditoria |
| `Builds/Windows-Visual-R2-20260922/` | 338 MB | OLDER_BUILD; documento R2 registra sucesso, 518 avisos | Revisar exclusão após confirmar que R4 a substitui visualmente |
| `Builds/Windows-Functional-V3-20260916/` | 351 MB | OLDER_BUILD / EVIDENCE_KEEP: `V3_FUNCTIONAL_VALIDATION.md` registra CT Windows 11/11 | Manter até transportar/confirmar evidência acadêmica |

O executável raiz data de agosto, mas os dados foram reconstruídos em 25/09. `D3D12`, `MonoBleedingEdge`, DLLs e `UnityPlayer.dll` são componentes adjacentes da build raiz, não lixo isolado. Não houve execução da build nesta auditoria.

## Documentos, evidências e ferramentas

- `docs/` CURRENT: `UNITY_SKILLS_PROMPT`, auditoria visual de 24/09, revisão de assets de 25/09, relatório visual R4 e contratos de integração. EVIDENCE: relatórios V3, QA, licença e `AssetCleanupReview/` (8,69 MB). AUDIT: `PROJECT_CLEANUP_AUDIT` e este documento. OUTDATED/REVIEW: handoff R2/R3 e planos antigos podem conter contexto único; não foi comprovada duplicata segura. TEMP confirmado: nenhum.
- `TestResults/` contém 171 arquivos. Vários XML/JSON/PNG são citados diretamente em `docs/FINAL_GAME_SYSTEM_MAP.md`, `STATISTICAL_QA_ANALYSIS.md`, `V3_CLOSURE_REPORT.md`, `V3_FUNCTIONAL_VALIDATION.md`, `VISUAL_R2_PROGRESS_2026-09-22.md` e `VISUAL_FINISH_R4_2026-09-25.md`. `VisualRebuild/` concentra 21,3 MB. Duplicatas e tentativas antigas exigem índice por referência antes de exclusão.
- `Logs/` contém 166 arquivos, inclusive logs históricos de build/testes e arquivos atuais (`relay.txt`, `traces.jsonl`, workers). Nenhum subconjunto foi demonstrado livre de valor probatório e de uso ativo nesta passada.
- `ArtStaging/`: R2 25,6 MB, R3 15,0 MB, R4 backups de cenas 8,65 MB, R5 cenas 9,02 MB. As 19 imagens fonte R2/R3 são material de edição único em formato/escala, mesmo que sprites derivados estejam em `Assets/Art`; `docs/ASSET_CLEANUP_REVIEW_2026-09-25.md` as registra como SOURCE_ONLY. R4/R5 contêm versões de cena fora de `Assets` que podem servir de recuperação. **58,3 MB é limite superior revisável, não espaço seguro para apagar agora.**
- `Tools/`: `ImportVisualR2.cs.txt` e `ImportVisualR3.cs.txt` leem `ArtStaging`; `ComposeMapsR4.cs.txt` e `RefinePresentationR4.cs.txt` são receitas editoriais documentadas em `VISUAL_FINISH_R4`; `GenerateNarrativeV3DialogueAssets.ps1` gera conteúdo narrativo. `Assets/Editor/` contém scripts Unity efetivos de build, montagem, validação e cenas; não confundir com receitas externas e não excluir nesta etapa.
- `.codex-checkpoints/` contém `ui-beta-before-integration.bundle` (44,4 MB), snapshot Git potencialmente único. `.agents/` contém 188 arquivos de skills; `AGENTS.md` e `skills-lock.json` fazem parte do fluxo atual. Revisar checkpoint após garantir substituto recuperável; manter instruções e skills.
- `.vscode/` contém `settings.json`, `extensions.json`, `launch.json`; configurações úteis de visualização, solução e attach ao Unity. `UserSettings/` contém apenas preferências/índices/layouts locais do Editor, sem conteúdo do jogo.
- `plastic4/` está vazia. `ProjectSettings/VersionControlSettings.asset` indica **Visible Meta Files** e o repositório usa Git; `com.unity.collab-proxy` está instalado, o que por si só não confirma uso de Unity Version Control. Classificação provisória TOOLING_OLD, pendente confirmação do fluxo da equipe.
- `develop/` não existe. `devlog/VISUAL_REBUILD_2026-09-16.md` é único e `CONTRIBUTING.md` exige registros em `devlog/`; manter.

## Plano de exclusão

- **Apagado agora com segurança:** `EvidenceDraft/` (somente subpasta vazia) e `.c5-final-evidence/` (vazia). Nenhum ganho mensurável em bytes.
- **Apagar depois de fechar Unity:** `Library/` (4,05 GB), `Temp/` (48 KB); `UserSettings/` (58 KB) e os quatro `.csproj` + `.slnx` (336 KB) são opcionais. Confirmar também IDE fechada para arquivos de solução. A Unity reimportará tudo ao reabrir; não há necessidade funcional de limpar `Library` se o objetivo for só organização visual.
- **Manter:** núcleo Unity, Git, documentação, devlog, build R4, ferramentas ativas e evidências citadas.
- **Revisar manualmente:** builds V3/R2, `TestResults/`, `Logs/`, `.codex-checkpoints/`, `ArtStaging/`, `plastic4/` e receitas editoriais após entrega.
- **Pode apagar depois da entrega:** checkpoint e builds antigas somente após confirmar retenção da evidência e possibilidade de recuperação; fontes de staging somente após decisão sobre futuras edições.

Espaço recuperável **agora, com exclusão comprovadamente segura:** 0 B. **Depois de fechar Unity:** até 4,047 GB de `Library` e 48 KB de `Temp`; ~0,4 MB opcionais de preferências/IDE. **Após revisão:** até 689 MB das duas builds antigas, 58,3 MB de staging e 44,4 MB do checkpoint, sem contar resultados e logs ainda não triados. Esses valores não devem ser somados como promessa de exclusão segura.
