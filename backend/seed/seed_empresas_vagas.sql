-- =====================================================================
-- Open Doors — dados de exemplo: empresas de vários nichos + vagas
-- Banco: PostgreSQL (RDS)
-- =====================================================================
-- Todas as empresas são FICTÍCIAS (nomes, CNPJs e e-mails inventados).
-- Essas empresas não têm login: servem só para popular a vitrine de vagas.
--
-- Pode rodar mais de uma vez: as empresas usam ids fixos e as vagas
-- delas são apagadas e recriadas a cada execução.
-- O contador empresas.vagas_ativas é atualizado pela trigger trg_vagas_ativas.
--
-- Para remover tudo depois:
--   delete from vagas    where empresa_id::text like '0d000000-0000-4000-8000-%';
--   delete from empresas where "Id"::text      like '0d000000-0000-4000-8000-%';
-- =====================================================================

begin;

-- ---------------------------------------------------------------------
-- 1) EMPRESAS (12 nichos diferentes)
-- ---------------------------------------------------------------------
insert into empresas
  ("Id", razao_social, nome_fantasia, cnpj, email, telefone, site,
   cidade, estado, cep, setor, porte, descricao,
   responsavel_nome, responsavel_cargo, responsavel_email,
   vagas_ativas, total_contratacoes, criado_por_admin, status, criado_em, atualizado_em)
select v.*, now(), now() from (values
  ('0d000000-0000-4000-8000-000000000001'::uuid, 'Byteforma Tecnologia Ltda', 'Byteforma',
   '11.111.111/0001-01', 'rh@byteforma.exemplo.com.br', '(11) 4000-1001', 'https://byteforma.exemplo.com.br',
   'São Paulo', 'SP', '01310-100', 'Tecnologia', 'Médio',
   'Software house que desenvolve sistemas web e aplicativos para pequenas e médias empresas.',
   'Carla Mendes', 'Coordenadora de RH', 'carla@byteforma.exemplo.com.br', 0, 0, true, 'ativa'),

  ('0d000000-0000-4000-8000-000000000002'::uuid, 'Clínica Vida Plena Serviços Médicos Ltda', 'Vida Plena Saúde',
   '22.222.222/0001-02', 'rh@vidaplena.exemplo.com.br', '(31) 4000-2002', 'https://vidaplena.exemplo.com.br',
   'Belo Horizonte', 'MG', '30130-000', 'Saúde', 'Médio',
   'Rede de clínicas de atendimento ambulatorial, exames e telemedicina.',
   'Rafael Souza', 'Gerente Administrativo', 'rafael@vidaplena.exemplo.com.br', 0, 0, true, 'ativa'),

  ('0d000000-0000-4000-8000-000000000003'::uuid, 'Cofre Azul Serviços Financeiros S.A.', 'Cofre Azul',
   '33.333.333/0001-03', 'talentos@cofreazul.exemplo.com.br', '(11) 4000-3003', 'https://cofreazul.exemplo.com.br',
   'São Paulo', 'SP', '04538-133', 'Finanças', 'Grande',
   'Fintech de conta digital e crédito para pessoas físicas e microempreendedores.',
   'Juliana Prado', 'Analista de Talentos', 'juliana@cofreazul.exemplo.com.br', 0, 0, true, 'ativa'),

  ('0d000000-0000-4000-8000-000000000004'::uuid, 'Rota Certa Logística e Transportes Ltda', 'Rota Certa',
   '44.444.444/0001-04', 'rh@rotacerta.exemplo.com.br', '(19) 4000-4004', 'https://rotacerta.exemplo.com.br',
   'Campinas', 'SP', '13010-000', 'Logística', 'Grande',
   'Operador logístico com centros de distribuição e entregas para e-commerce.',
   'Marcos Lima', 'Supervisor de RH', 'marcos@rotacerta.exemplo.com.br', 0, 0, true, 'ativa'),

  ('0d000000-0000-4000-8000-000000000005'::uuid, 'Saber Mais Educação Ltda', 'Saber Mais',
   '55.555.555/0001-05', 'contato@sabermais.exemplo.com.br', '(41) 4000-5005', 'https://sabermais.exemplo.com.br',
   'Curitiba', 'PR', '80010-000', 'Educação', 'Pequeno',
   'Escola de cursos livres e reforço escolar, presencial e online.',
   'Patrícia Gomes', 'Diretora Pedagógica', 'patricia@sabermais.exemplo.com.br', 0, 0, true, 'ativa'),

  ('0d000000-0000-4000-8000-000000000006'::uuid, 'Faísca Comunicação e Marketing Ltda', 'Agência Faísca',
   '66.666.666/0001-06', 'vagas@faisca.exemplo.com.br', '(21) 4000-6006', 'https://faisca.exemplo.com.br',
   'Rio de Janeiro', 'RJ', '20040-000', 'Marketing e Comunicação', 'Pequeno',
   'Agência de marketing digital, redes sociais e produção de conteúdo.',
   'Bruno Alves', 'Sócio-diretor', 'bruno@faisca.exemplo.com.br', 0, 0, true, 'ativa'),

  ('0d000000-0000-4000-8000-000000000007'::uuid, 'Mercado Bom Preço Comércio Ltda', 'Bom Preço Supermercados',
   '77.777.777/0001-07', 'rh@bompreco.exemplo.com.br', '(81) 4000-7007', 'https://bompreco.exemplo.com.br',
   'Recife', 'PE', '50010-000', 'Varejo', 'Grande',
   'Rede regional de supermercados com 25 lojas no Nordeste.',
   'Fernanda Rocha', 'Gerente de Pessoas', 'fernanda@bompreco.exemplo.com.br', 0, 0, true, 'ativa'),

  ('0d000000-0000-4000-8000-000000000008'::uuid, 'Campo Verde Agronegócios S.A.', 'Campo Verde',
   '88.888.888/0001-08', 'pessoas@campoverde.exemplo.com.br', '(34) 4000-8008', 'https://campoverde.exemplo.com.br',
   'Uberlândia', 'MG', '38400-000', 'Agronegócio', 'Grande',
   'Produção de grãos e consultoria em agricultura de precisão.',
   'Eduardo Martins', 'Coordenador de RH', 'eduardo@campoverde.exemplo.com.br', 0, 0, true, 'ativa'),

  ('0d000000-0000-4000-8000-000000000009'::uuid, 'Sabor da Terra Alimentos Ltda', 'Sabor da Terra',
   '99.999.999/0001-09', 'rh@sabordaterra.exemplo.com.br', '(51) 4000-9009', 'https://sabordaterra.exemplo.com.br',
   'Porto Alegre', 'RS', '90010-000', 'Alimentação', 'Médio',
   'Indústria de alimentos naturais e congelados.',
   'Luciana Ferreira', 'Analista de RH', 'luciana@sabordaterra.exemplo.com.br', 0, 0, true, 'ativa'),

  ('0d000000-0000-4000-8000-000000000010'::uuid, 'Alicerce Engenharia e Construções Ltda', 'Alicerce Engenharia',
   '10.101.010/0001-10', 'rh@alicerce.exemplo.com.br', '(61) 4000-1010', 'https://alicerce.exemplo.com.br',
   'Brasília', 'DF', '70040-000', 'Construção Civil', 'Médio',
   'Construtora de obras residenciais e comerciais.',
   'Ricardo Nunes', 'Engenheiro Responsável', 'ricardo@alicerce.exemplo.com.br', 0, 0, true, 'ativa'),

  ('0d000000-0000-4000-8000-000000000011'::uuid, 'Sol Nascente Energia Renovável Ltda', 'Sol Nascente Energia',
   '12.121.212/0001-11', 'rh@solnascente.exemplo.com.br', '(85) 4000-1111', 'https://solnascente.exemplo.com.br',
   'Fortaleza', 'CE', '60010-000', 'Energia', 'Médio',
   'Projetos e instalação de energia solar para residências e empresas.',
   'Aline Castro', 'Coordenadora Administrativa', 'aline@solnascente.exemplo.com.br', 0, 0, true, 'ativa'),

  ('0d000000-0000-4000-8000-000000000012'::uuid, 'Brisa Mar Hotelaria e Turismo Ltda', 'Hotel Brisa Mar',
   '13.131.313/0001-12', 'rh@brisamar.exemplo.com.br', '(71) 4000-1212', 'https://brisamar.exemplo.com.br',
   'Salvador', 'BA', '40010-000', 'Turismo e Hotelaria', 'Médio',
   'Hotel e agência de turismo receptivo na orla de Salvador.',
   'Gabriel Teixeira', 'Gerente de Operações', 'gabriel@brisamar.exemplo.com.br', 0, 0, true, 'ativa')
) as v
on conflict ("Id") do nothing;

-- ---------------------------------------------------------------------
-- 2) VAGAS (recria as vagas das empresas de exemplo)
--    Habilidades usam os mesmos nomes da lista do sistema
--    (HABILIDADES_OPCOES em opendoors/Areas/menu-empresa.js).
-- ---------------------------------------------------------------------
delete from vagas where empresa_id::text like '0d000000-0000-4000-8000-%';

insert into vagas
  (empresa_id, titulo, descricao, area, nivel, cursos_aceitos, semestre_minimo,
   habilidades_requeridas, habilidades_diferenciais, carga_horaria, modalidade,
   cidade, estado, bolsa, beneficios, vagas_disponiveis, candidaturas_recebidas,
   status, criado_em, expira_em, atualizado_em)
select v.empresa_id, v.titulo, v.descricao, v.area, v.nivel, v.cursos_aceitos, v.semestre_minimo,
       v.habilidades_requeridas, v.habilidades_diferenciais, v.carga_horaria, v.modalidade,
       v.cidade, v.estado, v.bolsa, v.beneficios, v.vagas_disponiveis, v.candidaturas_recebidas,
       v.status, now(), now() + make_interval(days => v.v_dias), now()
from (values
  -- Tecnologia — Byteforma
  ('0d000000-0000-4000-8000-000000000001'::uuid, 'Estágio em Desenvolvimento Web',
   'Apoiar o time no desenvolvimento de telas e integrações de sistemas web.',
   'Tecnologia', 'Estágio', array['Análise e Desenvolvimento de Sistemas','Ciência da Computação','Sistemas de Informação'], 2,
   array['HTML/CSS','JavaScript','Programacao'], array['Banco de dados','Ingles'],
   '6h/dia', 'Híbrido', 'São Paulo', 'SP', 1800::numeric,
   array['Vale-transporte','Vale-refeição','Auxílio home office'], 2, 0, 'aberta', 60),
  ('0d000000-0000-4000-8000-000000000001'::uuid, 'Estágio em Suporte Técnico',
   'Atender chamados de clientes e ajudar na configuração dos sistemas.',
   'Tecnologia', 'Estágio', array['Análise e Desenvolvimento de Sistemas','Redes de Computadores','Sistemas de Informação'], 1,
   array['Suporte tecnico','Atendimento ao cliente','Resolucao de problemas'], array['Banco de dados'],
   '6h/dia', 'Presencial', 'São Paulo', 'SP', 1500,
   array['Vale-transporte','Vale-refeição'], 1, 0, 'aberta', 45),
  ('0d000000-0000-4000-8000-000000000001'::uuid, 'Trainee em Análise de Dados',
   'Criar relatórios e dashboards para os clientes da empresa.',
   'Tecnologia', 'Trainee', array['Ciência da Computação','Sistemas de Informação','Estatística'], 6,
   array['Analise de dados','Excel','Power BI'], array['Banco de dados','Ingles'],
   '8h/dia', 'Remoto', 'São Paulo', 'SP', 3500,
   array['Vale-refeição','Plano de saúde','Auxílio home office'], 1, 0, 'aberta', 60),

  -- Saúde — Vida Plena
  ('0d000000-0000-4000-8000-000000000002'::uuid, 'Estágio em Enfermagem',
   'Acompanhar a equipe de enfermagem no atendimento ambulatorial.',
   'Saúde', 'Estágio', array['Enfermagem'], 4,
   array['Empatia','Trabalho em equipe','Organizacao'], array['Atendimento ao cliente'],
   '6h/dia', 'Presencial', 'Belo Horizonte', 'MG', 1400,
   array['Vale-transporte','Seguro de vida'], 3, 0, 'aberta', 45),
  ('0d000000-0000-4000-8000-000000000002'::uuid, 'Jovem Aprendiz — Recepção',
   'Receber pacientes, agendar consultas e organizar prontuários.',
   'Saúde', 'Jovem Aprendiz', array['Ensino Médio'], null,
   array['Atendimento ao cliente','Comunicacao','Organizacao'], array['Pacote Office'],
   '4h/dia', 'Presencial', 'Belo Horizonte', 'MG', 900,
   array['Vale-transporte','Curso de capacitação'], 2, 0, 'aberta', 30),

  -- Finanças — Cofre Azul
  ('0d000000-0000-4000-8000-000000000003'::uuid, 'Estágio em Finanças',
   'Apoiar a área financeira em conciliações, relatórios e análise de crédito.',
   'Finanças', 'Estágio', array['Administração','Ciências Contábeis','Economia'], 3,
   array['Financeiro','Excel','Pensamento analitico'], array['Power BI','Ingles'],
   '6h/dia', 'Híbrido', 'São Paulo', 'SP', 2000,
   array['Vale-refeição','Plano de saúde','Gympass'], 2, 0, 'aberta', 60),
  ('0d000000-0000-4000-8000-000000000003'::uuid, 'Estágio em Atendimento ao Cliente',
   'Atender clientes pelo chat e telefone, resolvendo dúvidas sobre a conta digital.',
   'Finanças', 'Estágio', array['Administração','Gestão Comercial','Marketing'], 1,
   array['Atendimento ao cliente','Comunicacao','Empatia'], array['Resolucao de problemas'],
   '6h/dia', 'Remoto', 'São Paulo', 'SP', 1600,
   array['Vale-refeição','Auxílio home office'], 4, 0, 'aberta', 45),

  -- Logística — Rota Certa
  ('0d000000-0000-4000-8000-000000000004'::uuid, 'Estágio em Logística',
   'Controlar estoque, acompanhar entregas e montar indicadores do centro de distribuição.',
   'Logística', 'Estágio', array['Logística','Administração','Engenharia de Produção'], 2,
   array['Logistica','Excel','Organizacao'], array['Power BI','Planejamento'],
   '6h/dia', 'Presencial', 'Campinas', 'SP', 1500,
   array['Vale-transporte','Refeição no local'], 2, 0, 'aberta', 45),
  ('0d000000-0000-4000-8000-000000000004'::uuid, 'Jovem Aprendiz — Auxiliar Administrativo',
   'Apoiar o escritório com notas fiscais, planilhas e arquivo.',
   'Logística', 'Jovem Aprendiz', array['Ensino Médio'], null,
   array['Organizacao','Pacote Office','Proatividade'], array['Excel'],
   '4h/dia', 'Presencial', 'Campinas', 'SP', 850,
   array['Vale-transporte','Curso de capacitação'], 3, 0, 'aberta', 30),

  -- Educação — Saber Mais
  ('0d000000-0000-4000-8000-000000000005'::uuid, 'Estágio em Pedagogia',
   'Auxiliar professores em turmas de reforço escolar.',
   'Educação', 'Estágio', array['Pedagogia','Letras'], 2,
   array['Comunicacao','Empatia','Planejamento'], array['Canva','Oratoria'],
   '4h/dia', 'Presencial', 'Curitiba', 'PR', 1100,
   array['Vale-transporte'], 2, 0, 'aberta', 45),
  ('0d000000-0000-4000-8000-000000000005'::uuid, 'Estágio em Conteúdo Educacional',
   'Produzir materiais didáticos e vídeos para os cursos online.',
   'Educação', 'Estágio', array['Letras','Pedagogia','Publicidade e Propaganda'], 3,
   array['Redacao','Canva','Criatividade'], array['Edicao de video'],
   '6h/dia', 'Remoto', 'Curitiba', 'PR', 1300,
   array['Auxílio home office'], 1, 0, 'aberta', 60),

  -- Marketing — Agência Faísca
  ('0d000000-0000-4000-8000-000000000006'::uuid, 'Estágio em Marketing Digital',
   'Planejar posts, acompanhar métricas e criar campanhas nas redes sociais.',
   'Marketing', 'Estágio', array['Marketing','Publicidade e Propaganda','Jornalismo'], 2,
   array['Marketing digital','Redacao','Criatividade'], array['Canva','Analise de dados'],
   '6h/dia', 'Híbrido', 'Rio de Janeiro', 'RJ', 1500,
   array['Vale-transporte','Day off no aniversário'], 2, 0, 'aberta', 60),
  ('0d000000-0000-4000-8000-000000000006'::uuid, 'Estágio em Design Gráfico',
   'Criar peças visuais para clientes da agência.',
   'Marketing', 'Estágio', array['Design Gráfico','Publicidade e Propaganda'], 2,
   array['Design grafico','Canva','Criatividade'], array['Edicao de video','Fotografia'],
   '6h/dia', 'Híbrido', 'Rio de Janeiro', 'RJ', 1500,
   array['Vale-transporte','Day off no aniversário'], 1, 0, 'aberta', 45),

  -- Varejo — Bom Preço
  ('0d000000-0000-4000-8000-000000000007'::uuid, 'Jovem Aprendiz — Operador de Loja',
   'Atendimento ao cliente, reposição e organização da loja.',
   'Varejo', 'Jovem Aprendiz', array['Ensino Médio'], null,
   array['Atendimento ao cliente','Trabalho em equipe','Proatividade'], array['Vendas'],
   '4h/dia', 'Presencial', 'Recife', 'PE', 850,
   array['Vale-transporte','Cesta básica','Curso de capacitação'], 5, 0, 'aberta', 30),
  ('0d000000-0000-4000-8000-000000000007'::uuid, 'Trainee em Gestão de Lojas',
   'Programa de 18 meses para formar futuros gerentes de loja.',
   'Varejo', 'Trainee', array['Administração','Gestão Comercial','Logística'], 7,
   array['Lideranca','Vendas','Gestao de projetos'], array['Analise de dados','Negociacao'],
   '8h/dia', 'Presencial', 'Recife', 'PE', 4000,
   array['Vale-refeição','Plano de saúde','Plano odontológico','Participação nos lucros'], 3, 0, 'aberta', 60),

  -- Agronegócio — Campo Verde
  ('0d000000-0000-4000-8000-000000000008'::uuid, 'Estágio em Agronomia',
   'Acompanhar plantio, análise de solo e uso de drones nas lavouras.',
   'Agronegócio', 'Estágio', array['Agronomia','Engenharia Agrícola'], 4,
   array['Pensamento analitico','Trabalho em equipe','Planejamento'], array['Analise de dados'],
   '6h/dia', 'Presencial', 'Uberlândia', 'MG', 1700,
   array['Moradia no local','Refeição no local','Transporte fretado'], 2, 0, 'aberta', 60),
  ('0d000000-0000-4000-8000-000000000008'::uuid, 'Estágio em Análise de Dados Agrícolas',
   'Organizar dados de produção e criar painéis de produtividade.',
   'Agronegócio', 'Estágio', array['Agronomia','Sistemas de Informação','Estatística'], 3,
   array['Analise de dados','Excel','Power BI'], array['Programacao'],
   '6h/dia', 'Híbrido', 'Uberlândia', 'MG', 1800,
   array['Vale-refeição','Transporte fretado'], 1, 0, 'aberta', 45),

  -- Alimentação — Sabor da Terra
  ('0d000000-0000-4000-8000-000000000009'::uuid, 'Estágio em Controle de Qualidade',
   'Apoiar análises de qualidade e segurança dos alimentos produzidos.',
   'Alimentação', 'Estágio', array['Engenharia de Alimentos','Nutrição','Química'], 3,
   array['Organizacao','Pensamento analitico','Trabalho em equipe'], array['Excel'],
   '6h/dia', 'Presencial', 'Porto Alegre', 'RS', 1400,
   array['Vale-transporte','Refeição no local'], 2, 0, 'aberta', 45),
  ('0d000000-0000-4000-8000-000000000009'::uuid, 'Estágio em Recursos Humanos',
   'Apoiar recrutamento, integração de novos colaboradores e ações internas.',
   'Recursos Humanos', 'Estágio', array['Psicologia','Gestão de Recursos Humanos','Administração'], 2,
   array['Recursos humanos','Comunicacao','Empatia'], array['Pacote Office'],
   '6h/dia', 'Presencial', 'Porto Alegre', 'RS', 1300,
   array['Vale-transporte','Refeição no local'], 1, 0, 'aberta', 60),

  -- Construção Civil — Alicerce
  ('0d000000-0000-4000-8000-000000000010'::uuid, 'Estágio em Engenharia Civil',
   'Acompanhar obras, medições e orçamentos junto ao engenheiro responsável.',
   'Engenharia', 'Estágio', array['Engenharia Civil','Arquitetura e Urbanismo'], 5,
   array['Planejamento','Excel','Resolucao de problemas'], array['Gestao de projetos'],
   '6h/dia', 'Presencial', 'Brasília', 'DF', 1900,
   array['Vale-transporte','Vale-refeição','Seguro de vida'], 2, 0, 'aberta', 60),

  -- Energia — Sol Nascente
  ('0d000000-0000-4000-8000-000000000011'::uuid, 'Estágio em Projetos de Energia Solar',
   'Dimensionar sistemas fotovoltaicos e apoiar visitas técnicas.',
   'Engenharia', 'Estágio', array['Engenharia Elétrica','Engenharia de Energia'], 4,
   array['Pensamento analitico','Excel','Planejamento'], array['Gestao de projetos'],
   '6h/dia', 'Presencial', 'Fortaleza', 'CE', 1600,
   array['Vale-transporte','Vale-refeição'], 2, 0, 'aberta', 45),
  ('0d000000-0000-4000-8000-000000000011'::uuid, 'Estágio em Vendas Consultivas',
   'Atender interessados, montar propostas e acompanhar negociações.',
   'Comercial', 'Estágio', array['Administração','Gestão Comercial','Marketing'], 2,
   array['Vendas','Negociacao','Comunicacao'], array['Oratoria'],
   '6h/dia', 'Híbrido', 'Fortaleza', 'CE', 1400,
   array['Vale-transporte','Comissão por venda'], 2, 0, 'aberta', 60),

  -- Turismo — Brisa Mar
  ('0d000000-0000-4000-8000-000000000012'::uuid, 'Estágio em Hotelaria',
   'Atuar na recepção, reservas e atendimento a hóspedes.',
   'Turismo', 'Estágio', array['Turismo','Hotelaria','Gestão de Eventos'], 1,
   array['Atendimento ao cliente','Ingles','Comunicacao'], array['Espanhol'],
   '6h/dia', 'Presencial', 'Salvador', 'BA', 1300,
   array['Vale-transporte','Refeição no local'], 3, 0, 'aberta', 45),
  ('0d000000-0000-4000-8000-000000000012'::uuid, 'Jovem Aprendiz — Eventos',
   'Apoiar a organização de eventos e passeios turísticos.',
   'Turismo', 'Jovem Aprendiz', array['Ensino Médio'], null,
   array['Trabalho em equipe','Organizacao','Comunicacao'], array['Espanhol','Fotografia'],
   '4h/dia', 'Presencial', 'Salvador', 'BA', 850,
   array['Vale-transporte','Curso de capacitação'], 2, 0, 'aberta', 30)
) as v (empresa_id, titulo, descricao, area, nivel, cursos_aceitos, semestre_minimo,
        habilidades_requeridas, habilidades_diferenciais, carga_horaria, modalidade,
        cidade, estado, bolsa, beneficios, vagas_disponiveis, candidaturas_recebidas,
        status, v_dias);

commit;

-- Conferência rápida
select e.setor, e.nome_fantasia, e.vagas_ativas, count(v."Id") as vagas
from empresas e
left join vagas v on v.empresa_id = e."Id"
where e."Id"::text like '0d000000-0000-4000-8000-%'
group by e.setor, e.nome_fantasia, e.vagas_ativas
order by e.setor;
