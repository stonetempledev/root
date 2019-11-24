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

-- toyn.test.articolo
select @id_parent = @id_element;
insert into [elements] (element_type, element_code, element_title) values ('articolo', 'articolo', 'Com''è cresciuto Barella, paratissime Gigio, è la squadra di Insigne');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);

insert into texts (text_content)
values ('Il commento : "E'' la squadra di Insigne" è fuorviante, in quanto è una libera e distorta rivisitazione del commento al punteggio riportato sulla Gazzetta, ovvero "Questa è la sua squadra". Ovvero, contro questa Bosnia in vacanza, con quei particolari giocatori a fianco, ha discretamente reso. Cioè il Napoli non è la sua squadra. E siamo d''accordo pienamente.Ancelotti non c''entra, anzi, a mio parere, poichè Insigne nel Napoli gioca come una pippa dall''inizio del campionato, peraltro scegliendosi il ruolo, l''allenatore dovrebbe sempre lasciarlo in panchina, come, peraltro, con un pizzico di coraggio, qualche settimana fa aveva iniziato a fare.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

-- toyn.test.articolo2
insert into [elements] (element_type, element_code, element_title) values ('articolo', 'articolo', 'Italia, decimo squillo: 3-0 alla Bosnia di Dzeko e Pjanic');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);

insert into texts (text_content)
values ('Ora sì, l''Europa comincia a temere l''Italia. Se Mancini cercava risposte sincere sul valore della Nazionale non può che sorridere: la Bosnia non sarà una grande, non è la Francia, non è la Spagna, ma non c’è dubbio che l''Italia l''abbia ridimensionata con una prova di grandissima personalità. Una superiorità schiacciante. Una di quelle partite che ti fanno diventare grande. E il 3-0 a Zenica (Acerbi, Insigne, Belotti) vale tante cose ben oltre il successo.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);
 
insert into titles (title_text) values ('RECORD, TESTA DI SERIE E...');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'title', @id_child);

insert into texts (text_content)
values ('Vale un posto in prima fascia al sorteggio della fase finale. Il record di 10 successi consecutivi, migliorando la striscia di 9 di Vittorio Pozzo negli anni 30. La sensazione di una squadra che gioca a memoria, si diverte e può fare a meno di Verratti perché il quasi debuttante Tonali è come se avesse sempre giocato qui. La magia nel recuperare gente come Bernardeschi, Insigne e Florenzi, tutti delusi dal campionato ma esaltanti in Nazionale. E se aggiungiamo che Donnarumma para letteralmente tutto, e che Belotti fa gol spettacolari, si può dire: abbiamo una Nazionale vera. Dove può arrivare non è chiaro ma nessun traguardo è impossibile.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

-- toyn.test.articolo2.articolo
select @id_parent = @id_element;
insert into [elements] (element_type, element_code, element_title) values ('articolo', 'articolo', 'Ansia Juve, oggi esami strumentali per Pjanic. Da valutare la sua presenza a Bergamo');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);

insert into texts (text_content)
values ('C''è un po'' di preoccupazione in casa Juve: in giornata Miralem Pjanic si sottoporrà ad esami strumentali, per definire l''entita dell''infortunio muscolare subito ieri (nella partita della Bosnia contro l''Italia) che lo ha costretto a lasciare il campo al 32'' della ripresa. Si saprà dunque a breve se il centrocampista sarà disponibile per la trasferta di Bergamo del prossimo 23 novembre.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

-- toyn.test.articolo2.articolo.articolo
select @id_parent = @id_element;
insert into [elements] (element_type, element_code, element_title) values ('articolo', 'articolo', 'Juve, assalto a Donnarumma. Ecco la strategia per l’estate');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);

insert into texts (text_content)
values ('Il primo tentativo, nel 2016, andò a vuoto. Ma la Juve non ha mai rinunciato all''idea di assicurarsi quello che per molti osservatori sarà il portiere del prossimo decennio azzurro: Gigio Donnarumma. E così il club bianconero sta lavorando con discrezione per portare un nuovo assalto, da concretizzare nella prossima estate.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

insert into texts (text_content)
values ('Nell’estate del 2013 è stato confermato dagli scienziati dell’Università di Lund (Svezia), che bombardando con ioni di calcio un sottile film di americio, crearono l’elemento 115, o meglio, la formazione di 4 atomi di ununpentium che scomparvero dopo 100 millisecondi dall’emissione di particelle alfa. Ma perché questo elemento può essere così importante?');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'text', @id_child);

-- toyn.test.articolo2.articolo.articolo2
insert into [elements] (element_type, element_code, element_title) values ('articolo', 'articolo', 'Nelle città sopra i 60 mila abitanti vincono i giallorossi, sotto il centrodestra');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);

insert into texts (text_content)
values ('C''è l''Italia dei comuni medio e grandi, quelli che hanno più di 60 mila abtitanti, che ha fiducia nell''attuale maggioranza di governo. E quella dei comuni sotto i 60 mila abitanti che premiano il centrodestra. Con Italia Viva di Matteo Renzi che nei centri più grandi quasi raddoppia i suoi consensi rispetto al dato nazionale. È la sintesi dei risultati di una rilevazione, a cura di Noto Sondaggi ed EMG Acqua, commissionata da ASMEL, l’Associazione nazionale per la Modernizzazione degli Enti Locali che rappresenta oltre 2800 Comuni italiani.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);

insert into texts (text_content)
values ('Di recente, cinque di questi ufficiali hanno annunciato che “persone sconosciute” con abiti scuri sono apparse poco dopo l’incidente degli UFO Nimitz, dichiarando che volevano tutto il materiale, ovvero, le registrazioni e i video degli incontri.Per diversi giorni nel novembre 2004, una nave della Marina degli Stati Uniti che navigava a 160 chilometri al largo della costa meridionale della California, ha rilevato segnali radar anomali provenienti da un oggetto nel cielo.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'text', @id_child);

-- toyn.test.articolo2.articolo.articolo3
insert into [elements] (element_type, element_code, element_title) values ('articolo', 'articolo', 'Proiettile in busta ad Antonio Conte: vigilanza per l’allenatore dell’Inter');
set @id_element = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_parent, 'element', @id_element);

insert into texts (text_content)
values ('Tecnicamente si parla di una vigilanza, il livello più basso di tutela per una personalità pubblica, ed è stata stabilita dopo avere esaminato l’entità della «minaccia». L’ex allenatore della Juventus e della Nazionale, passato alla guida della squadra nerazzurra dalla scorsa estate, qualche giorno fa ha infatti ricevuto una lettera anonima che conteneva una serie di minacce e un proiettile. È stato lo stesso allenatore a chiamare le forze dell’ordine e a firmare una denuncia contro ignoti, depositando la busta che aveva ricevuto. La società è stata avvertita dal tecnico e sta seguendo da vicino l’intera vicenda.');
set @id_child = @@IDENTITY;
insert into elements_contents (element_id, child_content_type, child_id) values (@id_element, 'text', @id_child);


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
