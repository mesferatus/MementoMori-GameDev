# Limpeza definitiva da raiz — fase 2 (25/09/2026)

## Resultado executado

| Estado | Caminho | Bytes liberados | Verificação e motivo |
|---|---|---:|---|
| REMOVIDO | `.codex-checkpoints/ui-beta-before-integration.bundle` e diretório pai | 44.407.928 | `git bundle verify` passou; contém somente HEAD `bbf04cd`, commit que já existe e é ancestral do HEAD atual; não guarda mudanças locais. Nenhuma documentação de produto depende do bundle. |
| REMOVIDO | `plastic4/` | 0 | Diretório vazio; projeto usa Git e `ProjectSettings/VersionControlSettings.asset` está em `Visible Meta Files`; nenhum script encontrado depende do diretório. A presença de `com.unity.collab-proxy` não implica workspace Plastic ativo. |

Na fase 1 já haviam sido removidas `.c5-final-evidence/` e `EvidenceDraft/`, sem arquivos e sem ganho em bytes. Não houve commit, push, reset ou alteração em `Assets/`, `Packages/` e `ProjectSettings/`.

**Espaço antes da fase 2:** 5.620.763.587 B (~5,62 GB). **Espaço medido após as remoções, antes de criar este relatório:** 5.576.355.659 B (~5,58 GB). **Liberado:** 44.407.928 B (~44,4 MB). A gravação deste relatório e mudanças simultâneas do Unity podem alterar ligeiramente a medição posterior. Soma lógica dos arquivos, incluindo `.git`.

## Build final e builds antigas

| Item | Tamanho | Classificação | Evidência preservada | Decisão |
|---|---:|---|---|---|
| `Builds/MementoMori-validated.exe` e dependências adjacentes | ~349 MB | FINAL / LATEST_VALIDATED | `TestResults/VisualRebuild/r4-build-summary.txt`, `r4-build-warnings.txt`, `r4-ct-windows-permitted.json`; `docs/VISUAL_FINISH_R4_2026-09-25.md` | MANTER: build R4 de 25/09, build Succeeded, CT Windows 11/11 registrado. Esta fase não repetiu execução. |
| `Builds/Windows-Visual-R2-20260922/` | 338.004.276 B | OLD_BUILD / REVIEW_REQUIRED | `TestResults/VisualRebuild/r2-build.txt`, `r2-ct-editor.json`, `r2-ct-windows.json`, `r2-imports.tsv`; `docs/VISUAL_R2_PROGRESS_2026-09-22.md` | Candidata a exclusão; contém uma versão visual histórica distinta. Confirmar que não é necessária como evidência executável. |
| `Builds/Windows-Functional-V3-20260916/` | 350.999.422 B | HISTORICAL_REQUIRED / REVIEW_REQUIRED | `TestResults/ct-functional-windows-authorized.json`, `functional-windows-authorized-exit.txt`, `functional-windows-player.log`; `docs/V3_FUNCTIONAL_VALIDATION.md` | Candidata a exclusão após decisão sobre reprodução do CT V3. Evidência textual já está preservada. |

As duas builds antigas somam **689.003.698 B**. Os diretórios de build não contêm relatórios únicos separados; os executáveis e dados são estados históricos reproduzíveis apenas por reconstrução, com risco de divergência. Por conterem evidência visual/funcional potencial, **não foram apagadas sem confirmação**, conforme seção 13 do pedido.

## TestResults — 171 mantidos, 0 removidos nesta fase

Inventário: 171 arquivos, 24.384.381 B. Busca por caminho ou nome nos documentos encontrou **42 citados** e **129 sem citação textual direta**. Falta de citação não prova obsolescência. Conjunto final proposto:

| Classe | Caminhos principais | Estado |
|---|---|---|
| FINAL_EVIDENCE | `VisualRebuild/editmode.xml`, `playmode.xml`, `r4-ct-editor-navigation.json`, `r4-ct-windows-permitted.json`, `r4-build-summary.txt`, `r4-build-warnings.txt` | Manter. O relatório R4 cita EditMode 25/25, PlayMode 13/13, CT Editor e Windows 11/11. |
| FINAL_EVIDENCE visual | `VisualRebuild/r4-mainmenu-final.png`, `r4-quarto.png`, `r4-labirinto.png`, `r4-lua-jardim.png`, `r4-lua-espelhos.png`, `r4-finalbeta-retorno.png`, `r4-finalbeta-endcard.png`, `r4-pause.png` | Manter para TCC; não houve nova inspeção visual nesta fase. |
| HISTORICAL_REQUIRED | `master-final-*.xml`, `editmode-manual-assets-20260819.xml`, `playmode-manual-assets-20260819.xml`, `ct-functional-*.json`, `functional-windows-*.log`, `VisualRebuild/r2-ct-*.json`, `r2-imports.tsv`, capturas V3 | Citados em documentação acadêmica; manter enquanto ela apontar para os arquivos. |
| DUPLICATE | `VisualRebuild/r4-dialogue.png` e `r4-quarto-current.png` têm SHA-256 idêntico; `VisualRebuild/editmode.xml` e `playmode.xml` também têm hash idêntico; `r3-DominioLua-decoration.txt` e `r3-Labirinto-decoration.txt` idem | Revisar: nomes e papéis diferem, e ambos XML são citados. Não excluir só pelo hash. |
| FAILED_ATTEMPT / TEMP / OBSOLETE candidatos | XML e logs `c3-*`, retries `c4*`/`c5*`, `ui-beta-*`, `r3-*`, capturas `*-before*`, intermediários `VisualRebuild/` não citados | Revisar em lote contra cobertura e conteúdo; nenhuma exclusão automática por risco de evidência histórica única. |

**Atenção:** os XML finais R4 de EditMode e PlayMode são byte a byte iguais embora o documento R4 informe contagens diferentes. A integridade dessa evidência precisa ser conferida antes de selecionar um conjunto final ou descartar outro resultado. A busca de citações por nome pode produzir falsos positivos; o relatório não trata 42 como lista definitiva de evidências.

## Logs — 166 mantidos, 0 removidos nesta fase

Inventário: 166 arquivos, 17.896.485 B; apenas oito nomes aparecem nos documentos, incluindo `final-build.log`, `final-playmode.log`, `master-final-*`, `relay.txt` e `traces.jsonl`. Os logs R4 da build estão principalmente em `TestResults/VisualRebuild`. `AssetImportWorker*.log` e `shadercompiler-*` estavam bloqueados por processos; `relay.txt` e `traces.jsonl` têm data atual. Há três processos Unity vivos. Logs históricos `c3*`, `c4*`, `c5*`, `ui-beta*` e `*-r2*` são candidatos a exclusão após triagem de conteúdo. **Não apagar `Logs/` com Unity aberto.**

## ArtStaging — 44 arquivos, 58.267.335 B mantidos

| Grupo | Tamanho | Classe | Decisão |
|---|---:|---|---|
| `VisualRebuildR2/` | 25.610.785 B | OLD_SOURCE + OLD_BACKUP + REVIEW_REQUIRED | 15 fontes PNG, catálogo e `slices.tsv` têm conteúdo de produção não equivalente aos recortes em `Assets/Art`. Cinco cenas backup antigas. Revisar fontes individualmente antes de excluir. |
| `VisualRefinementR3/` | 14.987.766 B | FINAL_SOURCE ou OLD_SOURCE + OLD_BACKUP | Quatro fontes PNG, `slices.tsv`, status e cinco cenas anteriores; `Assets/Editor/VisualRefinementR3.cs` ainda existe. Manter fontes até decidir necessidade de reedição. |
| `VisualFinalR4/SceneBackups/` | 8.649.360 B | OLD_BACKUP / CURRENT_BACKUP | Cinco snapshots anteriores ao R4, citados pelo relatório visual. Candidatos a arquivamento após escolher um único ponto de recuperação. |
| `VisualR5/Scenes/` | 9.019.424 B | DUPLICATE_OF_ASSETS / CURRENT_BACKUP | Cinco arquivos `.unity` com SHA-256 **igual** às cinco cenas correspondentes em `Assets/Scenes` na inspeção. Candidato mais claro à exclusão futura, mas requer confirmação por ser backup visual. |

As 19 fontes R2/R3 foram caracterizadas na auditoria visual de 24/09 como *source sheets* grandes, diferentes dos sprites runtime recortados. Nenhum PNG foi excluído.

## Tools e Assets/Editor

| Arquivo | Classe | Decisão |
|---|---|---|
| `Tools/GenerateNarrativeV3DialogueAssets.ps1` | ACTIVE_TOOL / ONE_TIME_SCRIPT | Manter por permitir regenerar assets narrativos. |
| `Tools/ImportVisualR2.cs.txt` | OLD_RECIPE | Revisar após decidir sobre fontes R2. |
| `Tools/ImportVisualR3.cs.txt` | OLD_RECIPE | Revisar após decidir sobre fontes R3. |
| `Tools/ComposeMapsR4.cs.txt` | CURRENT_RECIPE | Manter junto do relatório R4 até consolidar edição visual. |
| `Tools/RefinePresentationR4.cs.txt` | CURRENT_RECIPE | Manter pelo mesmo motivo. |

`Assets/Editor/` contém ferramentas Unity efetivas de build, validação, montagem e integração. Nenhum arquivo foi alterado ou excluído ali.

## Docs

- CURRENT_CANONICAL: `README.md`, `ARCHITECTURE.md`, contratos de integração, `FINAL_GAME_SYSTEM_MAP.md`, `ART_ASSET_REGISTRY.md` e `UNITY_SKILLS_PROMPT.md`.
- FINAL_EVIDENCE / FINAL_AUDIT: `V3_FUNCTIONAL_VALIDATION.md`, `V3_CLOSURE_REPORT.md`, `VISUAL_FINISH_R4_2026-09-25.md`, `VISUAL_ASSET_AUDIT_2026-09-24.md`, `ASSET_CLEANUP_REVIEW_2026-09-25.md`, `PROJECT_CLEANUP_AUDIT.md`, `AssetCleanupReview/` e este relatório.
- OUTDATED_BUT_UNIQUE / SUPERSEDED candidatos: `VISUAL_REBUILD_HANDOFF_2026-09-17.md`, `VISUAL_R2_PROGRESS_2026-09-22.md`, planos narrativos antigos, `PROJECT_BASELINE.md` e `ROADMAP.md`. Contêm decisões, referências de evidência ou histórico de produção; sem prova de substituição integral.
- Licença `third-party/Kenney-CC0-LICENSE.txt` e documentação TCC mantidas. Nenhum documento foi excluído.

## Ainda precisa decisão

1. Apagar as builds R2 e V3 (689 MB) após aceitar relatórios e CTs preservados como evidência suficiente, ou reter cópias executáveis históricas.
2. Escolher um backup de cenas para recuperação: `VisualR5/Scenes` é idêntico ao estado atual de `Assets/Scenes`; R4/R3/R2 têm estados anteriores. Decidir retenção antes de excluir backups ou fontes.
3. Confirmar o conjunto final de `TestResults`, especialmente a discrepância dos XML R4 e a lista de 129 arquivos sem citação textual.
4. Fechar todas as instâncias Unity/IDE para limpeza opcional de `Library/` (4,05 GB), `Temp/`, `UserSettings/` e arquivos de solução gerados. São caches locais, separados da redução permanente do projeto.

**Mantido:** núcleo do projeto, Git, instruções e skills, `README`, `CONTRIBUTING`, `devlog`, `Tools`, `docs`, `ArtStaging`, build R4, TestResults e Logs. **Plastic4:** removido. **Codex checkpoint:** removido. **Builds antigas:** preservadas aguardando decisão sobre evidência executável. **TestResults:** 171/0. **Logs:** 166/0. **ArtStaging:** 44/0. **Tools:** 5/0. **Docs:** preservados.
