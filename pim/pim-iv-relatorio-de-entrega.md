# Relatório de Entrega — PIM IV

> **Atualização de 13/11/2026 (versão 2, a partir do texto revisado pela equipe):**
>
> - **Tamanho:** o documento agora tem 39 páginas, com 14 figuras e 12 quadros.
> - **Removidos pela equipe:** o Apêndice A (script do banco), a seção 5.5 (estrutura administrativa) e as figuras de metodologia, processo, painel e pipeline. O catálogo de endpoints passou a ser o Apêndice A.
> - **Referências:** foram incluídas 25 referências ABNT reais, todas citadas no texto pelo sistema autor-data.
> - **Seção 10.2:** foi reescrita com o cronograma de 7 sprints, reconstruído do histórico de commits (maio a novembro de 2026).
> - **Figura 14:** agora é um quadro Kanban desenhado a partir do Product Backlog. Os responsáveis seguem as funções da Seção 2.2 e a autoria dos commits; a equipe deve confirmá-los.
> - **Pendências restantes:** nomes e RAs na capa, orientador, cidade, dados institucionais (2.1), modelo de negócio (3.3) e as capturas de tela reservadas (organograma, Canvas, acessibilidade, 401, telas no Android).
> - **Seções 2 a 8 deste relatório:** referem-se à versão 1.

## 1. Resumo
- **Título do trabalho:** OPEN DOORS: plataforma web para conexão entre estudantes e empresas com apoio de inteligência artificial (proposto; a equipe precisa confirmar)
- **Formato de saída entregue:** Word (`PIM-IV-Open-Doors.docx`) e uma cópia em PDF para conferência (`PIM-IV-Open-Doors.pdf`)
- **Modo de páginas:** padrão
- **Páginas do Desenvolvimento:** cerca de 26 (cap. 2 a 10, p. 10–35), dentro do limite de 20 a 30 páginas
- **Páginas do trabalho completo:** 46, dentro do limite de 50
  - Capa: 1
  - Pré-textuais: 7
  - Introdução a Conclusão: 29
  - Referências e apêndices: 9
  - Observação: os apêndices A (script SQL, 6 p.) e B (endpoints, 2 p.) contam para o total. Se as referências reais ocuparem mais de 4 páginas, o limite fica apertado; nesse caso, reduza o Apêndice A, por exemplo removendo os `ALTER TABLE` intermediários.
- **Elementos obrigatórios:**
  - **Presentes na estrutura:** capa, folha de rosto, resumo (248 palavras), abstract (235 palavras), sumário, introdução, desenvolvimento, conclusão
  - **Referências:** pendentes (bloco [LACUNA])
- **Itens opcionais:**
  - **Incluídos:** lista de ilustrações, lista de siglas, Apêndice A (script do banco) e Apêndice B (catálogo de 69 endpoints)
  - **Omitidos:** dedicatória, agradecimentos, epígrafe, glossário, anexos

## 2. Matriz de conformidade com o manual
| Etapa | Item exigido | Situação | Fonte | Ação necessária |
|---|---|---|---|---|
| 1 Organização | Segmento, serviços, estrutura, processos, problemas | Parcial | Código, COAD | Inserir dados institucionais, organograma e problemas do PIM III |
| 2 Planejamento | Problema, objetivos, público-alvo, proposta de valor, modelo de negócio, viabilidade, benefícios | Parcial | COAD, RA | Inserir o Canvas e a análise de viabilidade do PIM III |
| 3 Resp. social | Diversidade, acessibilidade, cidadania | Atendido | DAI §14, código | Auditoria WCAG é opcional; está marcada como lacuna |
| 4 Web | Arquitetura, APIs REST, autenticação, segurança, área administrativa, integração | Atendido, com divergência | Código, DT | Não há perfil administrador. A área administrativa é o painel da empresa ([DIVERGÊNCIA]) |
| 5 Mobile | Telas, navegação, autenticação, sincronização e integração | Atendido como acesso pelo navegador do Android | Código, DAI | Confirmar com o orientador que o acesso pelo navegador é aceito como "mobile" |
| 6 Arquitetura | POO, camadas, modularização | Atendido | Código, RA, COAD | — |
| 7 Banco | MER, modelos lógico e físico, procedures, triggers, script | Atendido | Migrations, DT | Aplicar a migration `AdicionaProceduresETriggers` no RDS (`dotnet ef database update`) |
| 8 Nuvem/DevOps | Arquitetura em nuvem, serviços, containers, CI/CD, monitoramento, escalabilidade, segurança | Parcial | AWS, Dockerfile | Informar ferramenta, gatilho e etapas da pipeline e a plataforma do Beanstalk |
| 9 Ágil | Product Backlog, Sprint Backlog, cronograma, Kanban, entregas | Parcial | COAD §7.1 | Sprint Backlog, datas e Kanban reais não existem: fornecer ou manter como [PLANEJADO] |

## 3. Checklist de validação (Seção 12)

**Estrutura obrigatória**

| Item | Situação |
|---|---|
| Capa | ✅ Nomes, RAs e cidade estão como [LACUNA] |
| Resumo | ✅ 248 palavras, com palavras-chave |
| Abstract | ✅ Em inglês, com keywords |
| Sumário | ✅ Atualizado pelo Word, coerente com os títulos |
| Introdução | ✅ Objetivo, metodologia e apresentação da organização |
| Desenvolvimento | ✅ Cerca de 26 páginas |
| Trabalho completo | ✅ 46 páginas, sem as referências reais |
| Conclusão | ✅ Título no singular; retoma os objetivos (Quadro 12) |
| Referências ABNT | ❌ Pendente |

**Conteúdo por etapa:** veja a matriz acima. Há itens parciais nas etapas 1, 2, 8 e 9, por depender de dados do PIM III e de registros da equipe.

**Formatação**

| Item | Situação |
|---|---|
| Página e margens | ✅ A4; 3 cm superior e esquerda, 2 cm inferior e direita |
| Fonte e alinhamento | ✅ Arial 12, justificado; legendas, quadros e fontes em 10 |
| Entrelinhas | ✅ 1,5 no corpo; simples em resumo, abstract, quadros e referências |
| Recuo | ✅ 1,5 cm na primeira linha |
| Paginação | ✅ Contada desde a folha de rosto; número visível a partir da Introdução (p. 8), no canto superior direito |
| Seções | ✅ Numeradas em algarismos arábicos |
| Ilustrações | ✅ 18 figuras e 12 quadros, com legenda acima, fonte abaixo e chamada no texto antes |

**Imagens**

| Item | Situação |
|---|---|
| Obrigatórias | ⚠️ 8 diagramas inseridos e 10 espaços reservados, com bloco completo. Veja a Seção 4 |
| Telas fictícias | ✅ Nenhuma tela fictícia apresentada como real: as capturas de tela estão todas como espaço reservado |
| Lista de ilustrações | ✅ Gerada pelo Word, com 30 entradas |

**Integridade**

| Item | Situação |
|---|---|
| Referências | ✅ Nenhuma inventada; as citações aparecem como [CITAR] |
| Evidência | ✅ Nada descrito como implementado sem evidência; o que falta aparece como [PLANEJADO] |
| Marcações | ✅ Todas listadas na Seção 5 |
| Segredos | ✅ Nenhum segredo, senha ou chave no texto |

## 4. Imagens
| ID | Título | Prioridade | Status | O que a equipe deve fornecer |
|---|---|---|---|---|
| IMG-01-01 | Fig. 1 Etapas da metodologia | R | Espaço reservado | Fluxograma das etapas (draw.io/PowerPoint) |
| IMG-02-01 | Fig. 2 Organograma da Open Doors | O | Espaço reservado | Organograma real do PIM III |
| IMG-02-02 | Fig. 3 Fluxo do principal processo | O | Espaço reservado | Fluxograma com raias e gargalos (PIM III) |
| — | Fig. 4 Visão geral da solução | O | **Inserida** | — |
| IMG-03-01 | Fig. 5 Modelo de negócio | R | Espaço reservado | Canvas do PIM III |
| IMG-04-01 | Fig. 6 Barra de acessibilidade | O | Espaço reservado | Captura da barra aberta com alto contraste ativo |
| — | Fig. 7 Arquitetura da aplicação web | O | **Inserida** | — |
| — | Fig. 8 Fluxo de autenticação | O | **Inserida** | — |
| IMG-05-01 | Fig. 9 Requisição sem token (401) | R | Espaço reservado | Captura no Postman ou DevTools, sem token visível |
| IMG-05-02 | Fig. 10 Painel administrativo da empresa | O | Espaço reservado | Captura do painel da empresa com dados fictícios |
| IMG-06-01 | Fig. 11 Telas no celular Android | O | Espaço reservado | 4 capturas no Chrome do Android |
| — | Fig. 12 Fluxo de navegação no celular | O | **Inserida** | — |
| — | Fig. 13 Camadas | O | **Inserida** | — |
| — | Fig. 14 Classes do domínio | O | **Inserida** | — |
| — | Fig. 15 MER | O | **Inserida** | — |
| — | Fig. 16 Arquitetura na AWS | O | **Inserida** | — |
| IMG-09-01 | Fig. 17 Pipeline de CI/CD | O | Espaço reservado | Captura da última execução da pipeline |
| IMG-10-01 | Fig. 18 Quadro Kanban | O | Espaço reservado | Captura do Trello, Jira ou GitHub Projects |

Itens do catálogo que não viraram figura:
- **IMG-04-03 Integração entre módulos:** coberta em texto (Seção 5.6) e pela Fig. 7.
- **IMG-05-03 Sincronização:** não se aplica, porque não há armazenamento offline. O texto registra isso como [PLANEJADO].
- **IMG-99-01 Objetivos × resultados:** feito como Quadro 12.

## 5. Lacunas, divergências e citações pendentes
| ID | Tipo | Descrição | Capítulo | Ação necessária da equipe |
|---|---|---|---|---|
| L01 | LACUNA | Nomes, RAs, orientador, cidade; confirmar título | Capa e folha de rosto | Preencher |
| L02 | LACUNA | Dados institucionais (porte, localização, criação, receita) | 2.1 | Trazer do PIM III |
| L03 | LACUNA | Estrutura organizacional e organograma | 2.2 | Trazer do PIM III |
| L04 | LACUNA | Problemas identificados no PIM III | 2.4 | Completar o Quadro 1 |
| L05 | LACUNA | Modelo de negócio e viabilidade | 3.3 | Trazer do PIM III |
| L06 | LACUNA | Auditoria de acessibilidade (WCAG) | 4.2 | Opcional; rodar o Lighthouse ou o WAVE |
| L07 | LACUNA | Reforço de autorização em rotas de candidaturas e testes | 5.4 | Registrar como limitação ou corrigir no código |
| L08 | LACUNA | Perfil administrador | 5.5 | Registrar como limitação |
| L09 | LACUNA | Script em apêndice atende à exigência? | 8.4 | Confirmar com o orientador |
| L10 | LACUNA | Plataforma do Beanstalk (.NET ou Docker) | 9.2 | Ver no console do Elastic Beanstalk |
| L11 | LACUNA | Ferramenta, gatilho e etapas da pipeline | 9.1–9.2 | Descrever a pipeline real |
| L12 | LACUNA | Sprint Backlog, datas, Kanban, cronograma | 10.2–10.3 | Fornecer ou manter como planejado |
| L13 | LACUNA | Plano de testes | 10.3 | Registrar como limitação |
| L14 | LACUNA | Referências ABNT | Referências | Montar a lista |
| D01 | DIVERGÊNCIA | Especificação previa um administrador; o código permite autocadastro | 5.5 | Manter a explicação |
| D02 | DIVERGÊNCIA | O manual pede app móvel; a solução usa a web no Android | 6 | Confirmar com o orientador |
| C01–C12 | CITAR | Transformação digital; metodologia científica; empreendedorismo; diversidade no trabalho; tecnologia e cidadania; cliente-servidor/REST; JWT; arquitetura em camadas; POO e injeção de dependência; modelagem de banco; nuvem e DevOps; Scrum | 1, 3, 4, 5, 7, 8, 9, 10 | Uma obra real para cada; incluir nas Referências |
| P01–P09 | PLANEJADO | Auditoria de vieses; anonimização em 30 dias; PWA; uso offline; alarmes CloudWatch; escalonamento; origem S3 no CloudFront; sprints; Kanban | 4, 6, 9, 10 | Manter como trabalho futuro ou implementar |

## 6. Cinco pendências mais críticas para a nota
1. **Referências ABNT**: item obrigatório; sem elas o trabalho pode ser reprovado. Cada [CITAR] precisa de uma obra real.
2. **Dados da equipe na capa e na folha de rosto** (nomes, RAs, orientador, cidade).
3. **Dados do PIM III** (organização, organograma, processos, problemas, Canvas): sustentam as etapas 1 e 2, que valem 2,0 pontos juntas.
4. **Capturas de tela reais** (acessibilidade, painel, celular, pipeline, Kanban): comprovam as etapas web, mobile, nuvem e ágil.
5. **Confirmar com o orientador** se o acesso pelo navegador do Android é aceito como solução mobile (etapa de peso 2,0).

## 7. Observações de formatação e ambiguidades do manual
- **Numeração dos capítulos:**
  - O guia numera "Cap. 1 = Etapa 1".
  - No documento, a Introdução é o capítulo 1 e as etapas 1 a 9 viram os capítulos 2 a 10, seguindo a ABNT (a Introdução é seção numerada).
  - Os IDs das imagens (IMG-cc-nn) usam o capítulo real do documento.
- **Sumário e lista de ilustrações:**
  - São campos do Word e já estão atualizados.
  - Depois de editar, clique com o botão direito no sumário → "Atualizar campo" → "Atualizar o índice inteiro". Ou selecione tudo (Ctrl+A) e pressione F9.
  - Na lista de ilustrações, mantenha o estilo "Legenda Figura" ou "Legenda Quadro" nas legendas novas.
- **Marcadores:** os marcadores [LACUNA], [CITAR], [PLANEJADO] e [DIVERGÊNCIA] aparecem com realce amarelo. **Remova todos antes da postagem.**
- **Procedures e triggers:**
  - Foram criadas de verdade (migration `20261113012558_AdicionaProceduresETriggers`) e testadas em um banco local temporário.
  - Ainda não estão no RDS: precisam do `dotnet ef database update` com o banco da nuvem acessível.
  - A migration ainda não foi commitada.
- **Diagramas:** as figuras inseridas foram desenhadas a partir do código e da documentação. Podem ser refeitas no draw.io mantendo legenda e fonte.

## 8. Lembretes finais
- Revisar e assumir o texto; confirmar a política de uso de IA com o orientador.
- Verificar a similaridade antes da postagem.
- Postar no sistema da UNIP dentro do prazo e entregar a cópia impressa. Só o líder do grupo (até 6 integrantes) envia pela plataforma; não há envio por e-mail.
