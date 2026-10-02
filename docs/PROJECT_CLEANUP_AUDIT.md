# Memento Mori — Project Cleanup Audit

Data da auditoria: 2026-09-13  
Referência remota observada: `origin/master` em `49f3face723ba93f71e8ab4499373ef57ea44f9b`

## Estado de sincronização

| Item | Resultado |
|---|---|
| Branch local | `master` |
| HEAD local | `49f3face723ba93f71e8ab4499373ef57ea44f9b` |
| `origin/master` | `49f3face723ba93f71e8ab4499373ef57ea44f9b` |
| Classificação | `LOCAL_EQUAL_REMOTE` |
| Fetch | Tentado; bloqueado pelo ambiente ao escrever `.git/FETCH_HEAD` |
| Commit/push | Não executados |

## Inventário e decisões

| PATH | TYPE | REFERENCED_BY | STATUS | ACTION | REASON |
|---|---|---|---|---|---|
| `Assets/Art/` | Arte runtime e sprites Unity | Cenas, prefabs e scripts por referências serializadas/nome | KEEP / CURRENT_V3 | Manter | Pacote de arte existente; nenhuma arte foi criada nesta execução. |
| `Assets/Art/Sprites/Quarto/` | Sprites de fase e estados | `Quarto.unity`, controllers e prefabs | CURRENT_V3 | Manter | Cobre estados de investigação, ritual e transição. |
| `Assets/Art/Sprites/Labirinto/` | Sprites de fase, puzzles e VFX | `Labirinto.unity`, controllers e prefabs | CURRENT_V3 | Manter | Pacote Unity-ready já presente; não duplicar. |
| `Assets/Art/Sprites/DominioDaLua/` | Sprites de jardim, espelhos, sigilo e estados | `DominioLua.unity`, puzzles e controllers | CURRENT_V3 | Manter | Cobre Delayed/Ahead/Room, sigilo e fragmento. |
| `Assets/Art/Sprites/VFX/` | Efeitos visuais | Cenas e controllers | CURRENT_V3 | Manter | Usado como suporte visual de portais, Poe, sigilo e interação. |
| `Assets/LuizaAssets/` | Fonte/arquivo de arte entregue | Referências de integração e histórico | KEEP_FOR_COMPATIBILITY | Não mover/apagar | A ausência de prova de que cada fonte foi substituída impede remoção segura. |
| `Assets/Scenes/MainMenu.unity` | Cena runtime | Build Settings | CURRENT_V3 | Manter | Entrada e retorno do fluxo oficial. |
| `Assets/Scenes/Quarto.unity` | Cena runtime | Build Settings | CURRENT_V3 | Manter | Primeira fase oficial. |
| `Assets/Scenes/Labirinto.unity` | Cena runtime | Build Settings | CURRENT_V3 | Manter | Segunda fase oficial. |
| `Assets/Scenes/DominioLua.unity` | Cena runtime | Build Settings | CURRENT_V3 | Manter | Terceira fase oficial. |
| `Assets/Scenes/FinalBeta.unity` | Epílogo runtime | Build Settings | CURRENT_V3 | Manter | Epílogo; não é fase 4. |
| `Assets/Resources/Dialogue/DLG_Q_*`, `DLG_L_*`, `DLG_D_*`, `DLG_F_*` | Diálogos V3 | `NarrativeV3DialogueCatalog`, bakers, cenas e triggers | CURRENT_V3 | Manter | Catálogo contém 82 IDs; são a fonte narrativa V3. |
| `Assets/Resources/Dialogue/DLG_ROOM_*`, `DLG_C4B_*`, `DLG_C4C_*` | Diálogos históricos | Cenas, prefabs, testes ou bindings antigos | LEGACY / UNCERTAIN_KEEP | Preservar | Há 152 assets totais e não foi provada a ausência de referência serializada. |
| `Assets/Scripts/Dialogue/NarrativeV3DialogueCatalog.cs` | Catálogo runtime | Testes e baker V3 | CURRENT_V3 | Manter | Fonte explícita dos 82 IDs V3. |
| `Assets/Editor/DialogueAssetsBaker.cs` | Ferramenta de geração | Menu/editor e catálogo | CURRENT_V3 | Manter | Mantém geração alinhada à Bíblia V3. |
| `Assets/Editor/SceneConfigurationBaker.cs` | Ferramenta de configuração | Cenas e bindings serializados | CURRENT_V3 | Manter | Regrava contratos V3; não é baker descartável. |
| `Assets/Tests/` | Cobertura EditMode/PlayMode | Unity Test Framework | CURRENT_V3 / LEGACY_MIXED | Auditar por resultado | Testes existentes incluem cobertura V3 e contratos históricos; sem licença não é seguro remover por nome. |
| `docs/` | Documentação técnica, acadêmica e histórica | Desenvolvimento, QA e avaliação | CURRENT / LEGACY_MIXED | Classificar, não apagar | Evidências e contratos antigos devem permanecer recuperáveis. |
| `Logs/`, `TestResults/`, `UI_Beta_Screenshots/`, `EvidenceDraft/` | Evidência/intermediários | QA local | GENERATED / DEBUG_ONLY | Manter fora do runtime; revisar depois | Não são assets runtime; não há autorização nem prova para exclusão material. |
| `.codex-checkpoints/`, `Tools/` | Artefatos de execução local | Processo de trabalho | GENERATED / UNCERTAIN_KEEP | Não incluir em limpeza automática | Conteúdo não foi demonstrado como obsoleto nesta passada. |

## Diálogos antigos

| OLD_ID/FAMÍLIA | NEW_V3_ID/FAMÍLIA | REFERENCES | ACTION |
|---|---|---|---|
| `DLG_ROOM_*` | `DLG_Q_*` | Possíveis bindings serializados e testes | `KEEP_ALIAS` até rebake/verificação no Editor |
| `DLG_C4B_*`, `DLG_LABYRINTH_*` | `DLG_L_*` | Histórico C4B e possíveis cenas | `KEEP_FOR_COMPATIBILITY` |
| `DLG_C4C_*`, `DLG_MOON_DOMAIN_*`, `DLG_MIRROR_*`, `DLG_SIGIL_*` | `DLG_D_*` | Histórico C4C e bindings antigos | `KEEP_FOR_COMPATIBILITY` |
| `DLG_ANDREALPHUS_02` | `DLG_D_ANDREALPHUS_02` | Referência runtime antiga observada | `KEEP_ALIAS`; asset antigo preservado até rebake |

## Resultado da limpeza

- Arquivos removidos: nenhum. Nenhum candidato atingiu `SAFE_TO_DELETE` com a evidência disponível.
- Arte criada: nenhuma.
- Arquivos mantidos por compatibilidade: fontes em `Assets/LuizaAssets/` e famílias de diálogos legados acima.
- Cenas oficiais: `MainMenu → Quarto → Labirinto → DominioLua → FinalBeta`.
- Candidatos para revisão posterior: relatórios intermediários em `Logs/`/`TestResults/`, diretórios de evidência e assets com sufixos históricos; exigem política de arquivamento e confirmação de referências.
- Estado de validação: a auditoria estática confirma a estrutura V3, mas rebake, console limpo e playthrough humano permanecem não comprovados nesta execução.

## Limitações

O Unity estava aberto, porém a validação automatizada registrada nos logs recentes continua limitada por licença/ambiente. O fetch não pôde atualizar `.git/FETCH_HEAD` por permissão do ambiente; os hashes disponíveis, entretanto, são iguais. Nenhuma conclusão de playthrough humano foi inferida.
