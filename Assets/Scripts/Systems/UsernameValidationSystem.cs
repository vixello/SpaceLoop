using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Systems
{

    /*    username_error_empty
    username_error_too_short
    username_error_too_long
    username_error_spaces
    username_error_starts_with_symbol
    username_error_symbol_only
    username_error_reserved
    username_error_brand
    username_error_offensive
    username_error_government
    username_error_financial
    username_error_generic
    */


    public class UsernameValidationSystem : IUsernameValidationSystem
    {
        private readonly HashSet<string> _reservedWords;
        private readonly HashSet<string> _brandProtected;
        private readonly HashSet<string> _offensive;
        private readonly HashSet<string> _government;
        private readonly HashSet<string> _financial;
        private readonly HashSet<string> _genericForbidden;
        private readonly HashSet<char> _symbols;

        public UsernameValidationSystem()
        {
            _symbols = new HashSet<char>
        {
            '#','-','_','0','1','2','3','4','5','6','7','8','9'
        };

            // ADMIN / SYSTEM / PRIVILEGED
            _reservedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "admin","administrator","administration","admins","adm","adm1n","4dmin",
            "root","rootuser","system","sys","sysop","sysadmin","systemadmin",
            "moderator","mod","modteam","supportteam","staff","staffadmin",
            "staffmanager","staffmember","team","teamadmin","teamowner",
            "super","superuser","superadmin","owner","hostadmin","hostmaster",
            "serveradmin","serverhost","server","host","hosting","hostserver",
            "operator","manager","master","supervisor","serviceaccount",
            "servicebot","systembot"
        };

            // BRAND-PROTECTED / COMPANY NAMES
            _brandProtected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "_appspecific",
            "danklens","danklenses","lens","lensadmin","lensarchive","lenscodes",
            "lenscodesadmin","lenses","virallenses","usersub","snap","snapcata",
            "snapchat","snapchatlens","snapchatlenses","snapinc","snaplens",
            "snaplenses","spectacles",
            "facebook","meta","instagram","tiktok","youtube","google","gmail",
            "whatsapp","telegram","discord",
            "paypal","stripe","visa","mastercard"
        };

            // OFFENSIVE / EXTREMIST / HATE
            _offensive = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "kkk","nazis","neo-nazi","hitler","fascist"
        };

            // GOVERNMENT / AUTHORITY / LEGAL
            _government = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "police","gov","government","embassy","customs","immigration","irs",
            "arpa"
        };

            // FINANCIAL / PAYMENT / BANKING
            _financial = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bank","banking","payments","payment","pay","payout","payouts",
            "billing","billingteam"
        };

            // EVERYTHING ELSE (GENERIC FORBIDDEN)
            _genericForbidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Symbols / single chars
            "a",

            // Generic forbidden
            "test","test1","test2","test3","test123","tester","tester1",
            "testftp","testuser","testuser1","testaccount",
            "username","user","usertest","useradmin","userftp","userusr",
            "me","you","null","unknown",

            // Web / tech / system / misc
            "about","abuse","access","account","accounts","add","address",
            "ads","adult","advertising","affiliate","affiliates","ajax","alias",
            "analytics","android","anon","anonymous","apache","apache2","api",
            "app","appowner","apps","appserver","archive","assets","asterisk",
            "atom","auth","authentication","automount","avatar","backup","banner",
            "banners","bash","beta","bin","blog","blogs","board","boarding","bot",
            "bots","business","cache","cadastro","calendar","campaign","card",
            "careers","categories","category","cgi","changelog","changeme","chat",
            "claim","client","clients","code","comercial","comment","comments",
            "compare","compras","config","connect","console","contact","content",
            "cores","cp","cpanel","create","creator","creators","css","curl",
            "customer","customers","daemon","dashboard","data","database","db",
            "delete","demo","deploy","design","designer","dev","devel","develop",
            "developer","dir","directory","discover","doc","docs","document",
            "documents","domain","download","downloads","ds_store","ecom",
            "ecommerce","edit","editor","email","etc","exec","exit","faq","fav",
            "favicon","favorite","fax","feed","feedback","file","files","firewall",
            "flog","follow","forum","forums","free","ftp","ftpadmin","ftpguest",
            "ftptest","ftpuser","ftpusr","gadget","gadgets","games","git","go",
            "goto","group","groups","guest","help","helpdesk","hidden","home",
            "homepage","hostserver","htaccess","html","htpasswd","http","httpd",
            "https","image","images","imap","img","index","indice","info",
            "install","internet","intranet","invite","ipad","iphone","irc","java",
            "javascript","job","jobs","js","kernal","knowledgebase","lib",
            "libexec","library","like","likes","link","links","linux","list",
            "lists","local","localhost","log","login","logout","logs","mach",
            "mach_kernal","mail","mail-exchange","mail1","mail2","mail3","mail4",
            "mail5","mail_exchange","mailer","mailexchange","mailing","mailman",
            "mailtest","marketing","media","message","messages","messenger",
            "microblog","microblogs","mine","mob","mobile","moodle","movie",
            "movies","mp3","msg","msn","music","musicas","mx","my","mysql",
            "nagios","name","named","net","network","new","news","newsletter",
            "nginx","nickname","noc","notes","noticias","ns","ns0","ns1","ns2",
            "ns3","ns4","office","old","onboarding","online","oracle","order",
            "orders","org","page","pager","pages","panel","passphrase","passwd",
            "password","perl","pgsql","photo","photoalbum","photos","php","pic",
            "pics","plugin","plugins","pop","pop3","post","postadmin","postfix",
            "postmaster","posts","preferences","privacy","privacy-policy",
            "privacy_policy","privacypolicy","private","prod","production",
            "profile","project","projects","promo","proxy","pub","public",
            "public_html","python","qwerty","random","register","registration",
            "restart","robots","rss","ruby","sale","sales","sample","samples",
            "sass","sbin","script","scripts","scss","search","secure","security",
            "send","serverhost","service","services","setting","settings","setup",
            "shell","shop","shutdown","signin","signout","signup","site",
            "sitemap","siteowner","sites","smtp","spam","sql","ssh","sshd",
            "stage","staging","standalone","start","stat","static","stats",
            "status","store","stores","student","students","su","subdomain",
            "submit","submitted","subscribe","sudo","suporte","sybase","sync",
            "systemuser","tablet","tablets","talk","task","tasks","tech","telnet",
            "telnetd","temp","temporary","terms","terms-of-service",
            "terms_of_service","termsofservice","theme","themes","tmp","todo",
            "tools","tos","trash","ts","tv","ubuntu","unix","update","upload",
            "url","usage","usenet","usuario","uucp","var","video","videos",
            "visitor","vm","vmail","vol","volumes","vv","vvv","w3","web",
            "webadmin","webmail","webmaster","webmin","website","websites",
            "webuser","welcome","win","workshop","wp-activate","wp-admin",
            "wp-blog-header","wp-comments-post","wp-config","wp-config-sample",
            "wp-content","wp-cron","wp-includes","wp-links-opml","wp-load",
            "wp-login","wp-mail","wp-settings","wp-signup","wp-trackback","ww",
            "wws","www","www-data","www1","www2","www3","www4","www5","www6",
            "www7","wwwrun","wwws","wwww","x","xhtml","xml","xpg","xxx","you",
            "yourdomain","yourname","yoursite","yourusername","zsh"
        };
        }

        public UsernameValidationResult Validate(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return Error(UsernameErrorType.Empty, "Username cannot be empty");

            if (username.Length < 3)
                return Error(UsernameErrorType.TooShort, "Username must be at least 3 characters");

            if (username.Length > 20)
                return Error(UsernameErrorType.TooLong, "Username cannot exceed 20 characters");

            if (username.Contains(" "))
                return Error(UsernameErrorType.ContainsSpaces, "Username cannot contain spaces");

            if (_symbols.Contains(username[0]))
                return Error(UsernameErrorType.StartsWithSymbol, "Username cannot start with a symbol or number");

            if (username.All(c => _symbols.Contains(c)))
                return Error(UsernameErrorType.SymbolOnly, "Username cannot consist only of symbols or numbers");

            string lower = username.ToLowerInvariant();

            if (_reservedWords.Contains(lower))
                return Error(UsernameErrorType.ReservedWord, "This username is reserved");

            if (_brandProtected.Contains(lower))
                return Error(UsernameErrorType.BrandProtected, "This username is protected by a brand");

            if (_offensive.Contains(lower))
                return Error(UsernameErrorType.Offensive, "This username contains offensive content");

            if (_government.Contains(lower))
                return Error(UsernameErrorType.Government, "This username cannot impersonate government entities");

            if (_financial.Contains(lower))
                return Error(UsernameErrorType.Financial, "This username cannot impersonate financial institutions");

            if (_genericForbidden.Contains(lower))
                return Error(UsernameErrorType.ForbiddenGeneric, "This username is not allowed");

            return new UsernameValidationResult
            {
                ErrorType = UsernameErrorType.Allowed,
                Message = null
            };
        }

        private UsernameValidationResult Error(UsernameErrorType type, string msg)
        {
            return new UsernameValidationResult
            {
                ErrorType = type,
                Message = msg
            };
        }

    }
}

