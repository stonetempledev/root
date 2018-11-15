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
            <li class="nav-item"><a class='nav-link' href="#els">COMANDI</a></li>
            <li class="nav-item"><a class='nav-link' href="#refs">RIFERIMENTI</a></li>
            <li id='lbl_docuteck' runat='server' class="nav-item"><a class='nav-link' href="#dtck">DOCUTECK</a></li>
          </ul>
        </div>
      </nav>
            <!-- contenuti -->
            <div class="col-md-9 ml-sm-auto px-4">
                <h1>
                    <span id='lbl_name' runat='server'></span>&nbsp;cosa vorresti fare?</h1>
                <p class="lead">
                    Scrivi <a id='view_cmds' runat='server' href='#'><b>'view cmds'</b></a> nella casella
                    'ESEGUI COMANDO' in alto e potrai vedere cosa puoi fare con theLantern.</p>
                <hr />
                <a id="els" class='anchor'></a>
                <h2>
                    COMANDI</h2>
                <p class='lead'>
                    Per usare theLantern, devi scrivere dei comandi nella casella di testo in alto.Devi
                    conoscere quali comandi puoi usare e come usarli per poter inserire, consultare
                    e ottenere le tue informazioni, in modo semplice e pratico.</p>
                <p class='lead'>
                    <u>Un comando è un insieme di parole chiave o istruzioni</u> che messe insieme danno
                    un ordine al theLantern. Ecco i tipi di parole chiave e la sequenza da utilizzare
                    per comporre un comando: <b>{azione comando} {oggetto comando} [{sotto-comando} {sotto-oggetto}]</b></p>
                <p class='lead'>
                    Le parole chiave di un comando devono essere separate da uno spazio, oppure se più
                    complesse possono essere comprese fra un apice o le doppie virgolette.</p>
                <p class='lead'>
                    Le varie istruzioni messe insieme nella giusta sequenza rappresentano un comando
                    da eseguire che fa una determinata cosa!</p>
                <ul>
                    <li>
                        <h4>
                            azione comando</h4>
                        <p class='lead'>
                            Parola chiave che indica l'azione principale del comando.</p>
                    </li>
                    <li>
                        <h4>
                            oggetto comando</h4>
                        <p class='lead'>
                            Elemento sulla quale viene eseguita l'azione. Ci sono oggetti predefiniti, ma <b>avrai
                                anche la possibilità di creare degli oggetti tuoi</b>, in base alle tue esigenze
                            e definirne i comportamenti. In questo modo potrai organizzare meglio e in modo
                            completo le tue informazioni!
                        </p>
                    </li>
                    <li>
                        <h4>
                            sotto-comando</h4>
                        <p class='lead'>
                            Ulteriore istruzione che va a completare l'azione principale.
                        </p>
                    </li>
                    <li>
                        <h4>
                            sotto-oggetto</h4>
                        <p class='lead'>
                            Ulteriore oggetto secondario che va a completare l'azione principale.
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
                    <li><a class='h3' href='https://github.com/thecreation/jquery-asBreadcrumbs' target='#'>jQuery asBreadcrumbs</a></li>
                    
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
