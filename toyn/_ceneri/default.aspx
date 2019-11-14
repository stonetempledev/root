<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="default.aspx.cs"
  Inherits="_default" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script language="javascript">
    $(document).ready(function () {
    });
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class="container-fluid">
    <div class="row">
      <!-- barra laterale -->
      <nav class="d-none d-md-block col-md-3 sidebar">
        <div class='sidebar-sticky'>
          <ul class="nav flex-column sticky-top">
            <li class="nav-item"><a class='nav-link' href="#els">COSÈ UN COMANDO</a></li>
            <li class="nav-item"><a class='nav-link' href="#refs">RIFERIMENTI</a></li>
            <li id='lbl_docuteck' runat='server' class="nav-item"><a class='nav-link' href="#dtck">DOCUTECK</a></li>
          </ul>
        </div>
      </nav>
      <!-- contenuti -->
      <div class="col-md-9 ml-sm-auto px-4">
        <h1>
          <b>T</b>emple <b>O</b>f <b>Y</b>our <b>N</b>otes</h1>
        <p class='lead'>
          Con il TOYN, puoi prendere appunti, creare la tua struttura dati, poter gestire
          tutta una serie di informazioni in modo facile ed immediato.<br />
          Quest'applicazione è nata e impostata per chi sviluppa software, ma è aperta a chiunque
          ne volesse sfruttare le potenzialità.
        </p>
        <a id="els" class='anchor'></a>
        <h2>
          Azione</h2>
        <p class='lead'>
          Il TOYN è in grado di interpretare tutta una serie di istruzioni chiamate <a id='view_cmds'
            runat='server' href='#'><b>azioni</b></a>, che se usate bene ti evitano di navigare
          e cliccare senza perderti fra le varie pagine e voci di menù.<br />
          Rappresentano l'insieme di tutte le informazioni e comandi che possono eseguiti
          da quest'applicazione.
          <br />
          Un'azione è un insieme di parole chiave ed elementi che legati fra loro nel
          modo giusto rappresentano un determinato comando da eseguire in un certo contesto.<br />
          Ovunque ti trovi, in qualsiasi pagina, se tu scrivi un'azione nella casella di testo
          in alto questa verrà eseguita, senza dover cercare la voce di menù o il pulsante
          specifico, basta ricordarsi i comandi giusti e fai quello che ti serve al momento.
        </p>
        <h3>
          <small>Simboli ed elementi basilari di un'azione</small></h3>
        <ul>
          <li><span class='h4'>{}</span>
            <p class='lead' style='display: inline'>
              parola chiave o istruzione
            </p>
          </li>
          <li><span class='h4'>&lt;&gt;</span>
            <p class='lead' style='display: inline'>
              classe o tipo di oggetto interessato dall'istruzione o parola chiave
            </p>
          </li>
          <li><span class='h4'>^^</span>
            <p class='lead' style='display: inline'>
              proprietà della classe
            </p>
          </li>
          <li><span class='h4'>+</span>
            <p class='lead' style='display: inline'>
              somma di più parole chiave o classi o insiemi.<br />
              Per sommare fra loro più <b>{parole chiave}</b> o <b>&lt;classi&gt;</b> si devono
              separare fra loro da uno spazio e se complesse possono essere comprese fra un apice
              o doppie virgolette.</p>
          </li>
          <li><span class='h4'>()</span>
            <p class='lead' style='display: inline'>
            raggruppamento di più parole chiave o classi che insieme hanno un particolare significato
          </li>
          <li><span class='h4'>[]</span>
            <p class='lead' style='display: inline'>
              parola chiave, classe o raggruppamento facoltativo</p>
          </li>
        </ul>
        <h3>
          <small>Com'è composta un'azione</small></h3>
        <p class='lead'>
          Ecco la formula completa di come può essere composta un'azione:
          <br />
          <b>({comando}+[&lt;classe comando&gt;])+[({sotto-comando}+[&lt;oggetto sotto-comando&gt;])]</b></p>
        <ul>
          <li>
            <h4>
              ({comando}+[&lt;oggetto comando&gt;])</h4>
            <p class='lead'>
              Comando principale ed elemento sulla quale viene eseguita quella determinata istruzione.<br />
              Può essere una classe predefinita, o personalizzata, cioè creata da te in base alle
              tue esigenze.<br />
              Per classe si intende la definizione di un certo gruppo di informazioni con delle
              caratteristiche particolari da te definite e legate fra loro in un certo modo.</p>
          </li>
          <li>
            <h4>
              [({sotto-comando}+[&lt;oggetto sotto-comando&gt;])]</h4>
            <p class='lead'>
              Sotto comando che può anch'esso essere associato ad una classe. Ha un senso compiuto
              solo se legato a un comando principale.
            </p>
          </li>
        </ul>
        <hr />
        <a id="refs" class='anchor'></a>
        <h2>
          RIFERIMENTI</h2>
        <ul>
          <li><a class='h3' href='https://getbootstrap.com' target='#'>bootstrap 4.0</a></li>
          <li><a class='h3' href='https://popper.js.org/' target='#'>popper.js</a></li>
          <li><a class='h3' href='https://mm.onokumus.com/' target='#'>metismenu</a></li>
          <li><a class='h3' href='https://github.com/thecreation/jquery-asBreadcrumbs' target='#'>
            jQuery asBreadcrumbs</a></li>
        </ul>
        <div id='sec_dtck' runat='server'>
          <hr />
          <a id="dtck" class='anchor'></a>
          <h2>
            DOCUTECK</h2>
          <p class='lead'>
            Dizionario tecnico strutturato per l'amministratore e lo sviluppatore.
          </p>
          <div id='body_dtk' runat='server'>
          </div>
        </div>
      </div>
    </div>
  </div>
</asp:Content>
