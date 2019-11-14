-- documento di test
truncate table [elements];
truncate table elements_contents;
truncate table titles;
truncate table texts;
truncate table [values];
truncate table accounts;
go

declare @id_element int;
declare @id_parent int;
declare @id_child int;

-- toyn
insert into [elements] (element_type, element_code, element_title) values ('app', 'toyn', 'Temple Of Your Notes');
set @id_element = @@IDENTITY;

insert into texts (text_content)
 values ('Con il TOYN, puoi prendere appunti, creare la tua struttura dati, poter gestire tutta una serie di informazioni in modo facile ed immediato.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

insert into texts (text_content)
 values ('Quest''applicazione è nata e impostata per chi sviluppa software, ma è aperta a chiunque ne volesse sfruttare le potenzialità.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

-- toyn.azione
select @id_parent = element_id from [elements] where element_code = 'toyn';
insert into [elements] (element_type, element_code, element_title, element_ref) values ('element', 'azione', 'Azione', '{@cmdurl=''view cmds''}');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element)

insert into texts (text_content)
 values ('Il TOYN è in grado di interpretare tutta una serie di istruzioni chiamate azioni
 , che se usate bene ti evitano di navigare
 e cliccare senza perderti fra le varie pagine e voci di menù.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

insert into texts (text_content)
 values ('Rappresentano l''insieme di tutte le informazioni e comandi che possono eseguiti da quest''applicazione.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

insert into texts (text_content, text_style)
 values ('Un''azione è un insieme di parole chiave ed elementi che legati fra loro nel
 modo giusto rappresentano un determinato comando da eseguire in un certo contesto.', '[underline]');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

insert into texts (text_content)
values ('Ovunque ti trovi, in qualsiasi pagina, se tu scrivi un''azione nella casella di testo
 in alto questa verrà eseguita, senza dover cercare la voce di menù o il pulsante
 specifico, basta ricordarsi i comandi giusti e fai quello che ti serve al momento.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

-- toyn.azione.simboli
select @id_parent = element_id from [elements] where element_code = 'azione';
insert into [elements] (element_type, element_code, element_title) values ('dtk', 'simboli', 'Simboli ed elementi basilari di un''azione');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);
 
-- toyn.azione.simboli.{}
select @id_parent = element_id from [elements] where element_code = 'simboli';
insert into [elements] (element_type, element_title) values ('syntax', '{}');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element)

insert into texts (text_content) values ('parola chiave o istruzione'); 
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

-- toyn.azione.simboli.<>
insert into [elements] (element_type, element_title) values ('syntax', '<>');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element)

insert into texts (text_content)
values ('classe o tipo di oggetto interessato dall''istruzione o parola chiave');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

-- toyn.azione.simboli.^^
insert into [elements] (element_type, element_title) values ('syntax', '^^');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element)

insert into texts (text_content) values ('proprietà della classe');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

-- toyn.azione.simboli.+
insert into [elements] (element_type, element_title) values ('syntax', '+');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element)

insert into texts (text_content)
values ('Somma di più parole chiave o classi o insiemi.
Per sommare fra loro più {parole chiave} o <classi> si devono separare fra loro da uno spazio e se complesse possono essere comprese fra un apice
 o doppie virgolette.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

-- toyn.azione.simboli.()
insert into [elements] (element_type, element_title) values ('syntax', '()');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element)

insert into texts (text_content)
values ('raggruppamento di più parole chiave o classi che insieme hanno un particolare significato');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

-- toyn.azione.simboli.[]
insert into [elements] (element_type, element_title) values ('syntax', '[]');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element)

insert into texts (text_content)
values ('parola chiave, classe o raggruppamento facoltativo');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);




-- toyn.azione.composizione
select @id_parent = element_id from [elements] where element_code = 'azione';
insert into [elements] (element_type, element_code, element_title) values ('dtk', 'composizione', 'Com''è composta un''azione');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);

insert into texts (text_content)
values ('Ecco la formula completa di come può essere composta un''azione:');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

insert into texts (text_content, text_style)
values ('({comando}+[<classe comando>])+[({sotto-comando}+[<oggetto sotto-comando>])]', '[bold]');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

-- toyn.azione.composizione.({comando}+[<oggetto comando>])
select @id_parent = element_id from [elements] where element_code = 'composizione';
insert into [elements] (element_type, element_title) values ('syntax', '({comando}+[<oggetto comando>])');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);

insert into texts (text_content)
values ('Comando principale ed elemento sulla quale viene eseguita quella determinata istruzione.
 Può essere una classe predefinita, o personalizzata, cioè creata da te in base alle tue esigenze.
 Per classe si intende la definizione di un certo gruppo di informazioni con delle caratteristiche particolari da te definite e legate fra loro in un certo modo.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);


-- toyn.azione.composizione.[({sotto-comando}+[<oggetto sotto-comando>])]
insert into [elements] (element_type, element_title) values ('syntax', '[({sotto-comando}+[<oggetto sotto-comando>])]');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);

insert into texts (text_content)
values ('Sotto comando che può anch''esso essere associato ad una classe. Ha un senso compiuto solo se legato a un comando principale.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);


-- toyn.riferimenti
select @id_parent = element_id from [elements] where element_code = 'toyn';
insert into [elements] (element_type, element_code, element_title) values ('refs', 'riferimenti', 'Riferimenti');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);

insert into [elements] (element_type, element_title, element_ref) values ('website', 'bootstrap 4.0', 'https://getbootstrap.com')
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'element', @id_child);

insert into [elements] (element_type, element_title, element_ref) values ('website', 'popper.js', 'https://popper.js.org/')
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'element', @id_child);

insert into [elements] (element_type, element_title, element_ref) values ('website', 'metismenu', 'https://mm.onokumus.com/')
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'element', @id_child);

insert into [elements] (element_type, element_title, element_ref) values ('website', 'jQuery asBreadcrumbs', 'https://github.com/thecreation/jquery-asBreadcrumbs')
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'element', @id_child);

-- toyn.test
select @id_parent = element_id from [elements] where element_code = 'toyn';
insert into [elements] (element_type, element_code, element_title) values ('test', 'test', 'Test element');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);

insert into titles (title_text) values ('Vetrina');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'title', @id_child);

insert into texts (text_content)
values ('Lo sbarco di Anzio (nome in codice operazione Shingle) fu un''operazione militare di sbarco anfibio, condotta dagli Alleati sulla costa tirrenica antistante gli abitati di Anzio e Nettuno, durante la campagna d''Italia nella seconda guerra mondiale. L''obiettivo di tale manovra era la creazione di una testa di ponte ad Anzio oltre lo schieramento tedesco sulla linea Gustav, in modo tale da aggirarla e costringere gli avversari a distogliere ingenti forze dal fronte di Cassino, permettendo così lo sfondamento della 5ª Armata del generale Mark Clark lungo il settore tirrenico della Gustav. In contemporanea, le truppe sbarcate ad Anzio avrebbero occupato i colli Albani, impedendo la ritirata delle divisioni tedesche: la loro distruzione avrebbe consentito di conquistare Roma e abbreviare la campagna.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

insert into titles (title_text) values ('Voci di qualità');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'title', @id_child);

insert into texts (text_content)
values ('Meneghino (in milanese Meneghin) è un personaggio del teatro milanese, ideato da Carlo Maria Maggi e divenuto in seguito maschera della commedia dell''arte. Prendendo il posto di Beltrame, è divenuto il simbolo popolaresco della città di Milano, tanto che il termine meneghino è normalmente utilizzato per identificare i cittadini milanesi.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

insert into titles (title_text) values ('Premi Nobel 2019');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'title', @id_child);

insert into [values] (value_content, value_notes) values ('William Kaelin Jr.', 'gassolinna');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'value', @id_child);

insert into [values] (value_content, value_ref, value_notes) values ('Peter J. Ratcliffe', 'http://www.google.it', 'ma dde chè?');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'value', @id_child);

insert into [values] (value_content) values ('Gregg L. Semenza');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'value', @id_child);

insert into titles (title_text) values ('...e i morti');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'title', @id_child);

insert into [accounts] (account_user, account_password, account_notes) values ('Ugo', 'Tognazzi', 'cremunees');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'account', @id_child);

insert into [accounts] (account_user, account_password) values ('Enrico', 'Mattei');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'account', @id_child);

insert into [accounts] (account_user, account_password) values ('Lise', 'Meitner');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'account', @id_child);

go

/*

select * from [elements];
select * from elements_contents;
select * from titles;
select * from texts;
select * from [values];
select * from accounts;

-- select * from vw_elements
-- open_element 1

*/
