
/************************/
/*** Load Youtube APi ***/
/************************/
var tag = document.createElement('script');
tag.src = "https://www.youtube.com/iframe_api";
var firstScriptTag = document.getElementsByTagName('script')[0];
firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);


/********************************/
/*** Youtube Player functions ***/
/********************************/
//Must be global scope.

var syncViewPlayer;

function onYouTubeIframeAPIReady() {
    syncViewPlayer = new YT.Player('player', {
        height: '1',
        width: '1',
        videoId: syncViewData.CurrentUrl,
        events: {
            'onReady': onsyncViewPlayerReady,
            'onStateChange': onsyncViewPlayerStateChange
        }
    });
};

function onsyncViewPlayerReady(event) {
    event.target.playVideo();
    syncViewPlayer.seekTo(parseFloat(syncViewData.CurrentTime));
    syncViewHelper.scrollToUrl(syncViewData.CurrentUrl);
};

function onsyncViewPlayerStateChange(event) {
    if (event.data == YT.PlayerState.ENDED) {
        if (syncViewData.IsRepeat) {
            syncViewPlayer.seekTo(0);
        }
        else if (syncViewData.IsShuffle) {
            syncViewHub.hub.server.shuffleUrl();
        }
        else {
            syncViewHub.hub.server.nextUrl();
        }
    }
    if (event.data == YT.PlayerState.PLAYING) {
        syncViewHub.hub.server.goToTime(syncViewPlayer.getCurrentTime(), syncViewData.GlobalRowV);
    }
};



/*********************************************/
/*** Hub init and server/client functions  ***/
/*********************************************/
var syncViewHub = {
    hub: $.connection.syncHub,
    youtubeUrlRegex: /^.*(?:(?:youtu\.be\/|v\/|vi\/|u\/\w\/|embed\/)|(?:(?:watch)?\?v(?:i)?=|\&v(?:i)?=))([^#\&\?]*).*/,
    init: function () {
        $.connection.hub.start().done(function () {
            // Bindklick events after the Websocket is open: to ensure response.
            $('#addurl').click(function () {
                this.blur();
                var inputUrl, video_id;
                inputUrl = $('#url').val();

                if (inputUrl.length > 11) {
                    video_id = inputUrl.match(youtubeUrlRegex);
                    googleApiHelper.getMetaDataByVideoID(video_id);
                }
                else if (inputUrl.length === 11) {
                    googleApiHelper.getMetaDataByVideoID(inputUrl);
                }
                else {
                    $('#url').val('BAD FORMAT');
                }
            });

            $('#nexturl').click(function () {
                this.blur();
                syncViewHub.hub.server.nextUrl();
            });

            $('#shuffleurl').click(function () {
                this.blur();
                syncViewHub.hub.server.initShuffleUrl();
            });

            $('#deletecurrenturl').click(function () {
                syncViewHub.hub.server.deleteUrl(syncViewData.CurrentUrl);
            });


            $('#urlListUL').on('click', '.repeatUrl', function () {
                if (syncViewData.IsRepeat)
                    syncViewHub.hub.server.setRepeat(syncViewData.CurrentUrl, false);
                else
                    syncViewHub.hub.server.setRepeat(syncViewData.CurrentUrl, true);
            });

            $('#urlListUL').on('click', 'li', function () {
                var self = $(this);
                if (syncViewData.CurrentUrl !== self.attr("id")) {
                    syncViewHub.hub.server.goToUrl(self.attr("id"));
                }
            });

            $("#fuzzysearch").keyup(function () {
                var i, len, title, urls, query, relevance, ra, rb, elems, obj, results;
                query = $("#fuzzysearch").val();
                if (query.length < 1 || query.length > 19)
                    return;

                urls = syncViewData.JsonUrlList;

                //Array of li :s  remove dom objects Weight and sort and then insert into dom.
                elems = $('#urlListUL').children('li').remove();

                for (i = 0, len = urls.length; i < len; i++) {
                    title = urls[i].Title;
                    relevance = stringRelevanceHelper.string_similarity(query, title);
                    //Normalize relevance
                    if (relevance < 0.1)
                        relevance = 0

                    elems.filter("#" + urls[i].UrlPart)[0].dataset.relevance = relevance.toFixed(2);
                }

                function cmp(a, b) { return a == b ? 0 : a < b ? 1 : -1; }
                elems.sort(function (a, b) {
                    //var a = $('#mydiv').data('myval');
                    ra = a.dataset.relevance;
                    rb = b.dataset.relevance;

                    return cmp(parseFloat(ra), parseFloat(rb)); //ra - rb;
                });

                $('#urlListUL').html(elems);
                syncViewHelper.scrollToUrlFaster(elems[0].id);
            });
        });
    }
};
syncViewHub.init();

// Clientside functions
syncViewHub.hub.client.addUrl = function (url, title) {
    $('#urlListUL').append(syncViewHelper.makeNewListItem(url, title));
    syncViewData.JsonUrlList.push({
        UrlPart: url,
        Title: title
    });
};

syncViewHub.hub.client.addUrlEx = function () {
    $('#url').val('BAD FORMAT');
};

syncViewHub.hub.client.nextUrl = function (returnUrl) {
    syncViewData.CurrentUrl = returnUrl;
    syncViewHelper.syncUrlList(syncViewData.CurrentUrl);
    syncViewData.IsRepeat = false;
    syncViewPlayer.stopVideo();
    syncViewPlayer.loadVideoById(syncViewData.CurrentUrl);
    syncViewHelper.scrollToUrl(syncViewData.CurrentUrl);
};

syncViewHub.hub.client.goToTime = function (time, version) {
    syncViewData.GlobalRowV = version;
    syncViewData.CurrentTime = time;
    syncViewPlayer.seekTo(parseFloat(syncViewData.CurrentTime));
};

syncViewHub.hub.client.goToUrl = function (returnUrl) {
    syncViewData.CurrentUrl = returnUrl;
    syncViewHelper.syncUrlList(syncViewData.CurrentUrl);
    syncViewData.IsRepeat = false;
    syncViewPlayer.stopVideo();
    syncViewPlayer.loadVideoById(syncViewData.CurrentUrl);
    syncViewHelper.scrollToUrl(syncViewData.CurrentUrl);
};

syncViewHub.hub.client.afterDelete = function (returnUrl, deleteThis) {
    $('#' + deleteThis).remove();
    //Delete Json item from search array
    syncViewData.JsonUrlList = $.grep(syncViewData.JsonUrlList, function (e) { return e.UrlPart != deleteThis });

    syncViewData.CurrentUrl = returnUrl;
    syncViewHelper.syncUrlList(syncViewData.CurrentUrl);
    syncViewData.IsRepeat = false;
    syncViewPlayer.stopVideo();
    syncViewPlayer.loadVideoById(syncViewData.CurrentUrl);
    syncViewHelper.scrollToUrl(syncViewData.CurrentUrl);
};

syncViewHub.hub.client.setRepeat = function (setBool, setUrl) {
    syncViewData.IsRepeat = setBool;

    if (syncViewData.IsRepeat) {
        syncViewData.IsShuffle = false;
        $("#shuffleurl").removeClass("isShuff");
    }

    var self = $("#" + syncViewData.CurrentUrl);
    self.find('.repeatUrl').toggleClass("cloroedRep");
};

syncViewHub.hub.client.initShuffleUrl = function (returnUrl, setBool) {
    syncViewData.IsShuffle = setBool;
    $("#shuffleurl").toggleClass("isShuff");

    if (setBool) {
        syncViewData.CurrentUrl = returnUrl;
        syncViewHelper.syncUrlList(syncViewData.CurrentUrl);
        syncViewData.IsRepeat = false;
        syncViewPlayer.stopVideo();
        syncViewPlayer.loadVideoById(syncViewData.CurrentUrl);
        syncViewHelper.scrollToUrl(syncViewData.CurrentUrl);
    }
};



/***************************/
/*** GoogleApi functions ***/
/***************************/
var googleApiHelper = {
    getMetaDataByVideoID: function(video_id) {
        if (video_id != null) {
            video_id = video_id[1];
            $.getJSON("https://www.googleapis.com/youtube/v3/videos?id=" + video_id + "&key=AIzaSyDQMjfzyAgCOJ6opn-loMv9_B_ztwZQXG4&fields=items%28id,snippet%28title%29%29&part=snippet", function () {
            }).success(function (data, status, xhr) {
                syncViewHub.hub.server.addUrl(data.items[0].id, data.items[0].snippet.title);
                $('#url').val('');
            })
            .error(function () {
                $('#url').val('error');
            });
        } else {
            $('#url').val('BAD FORMAT');
        }
    }
};

/********************************/
/*** Generic helper functions ***/
/********************************/
var syncViewHelper = {
    makeNewListItem: function (url, title) {
        var data, tpl;
        data = { url: url, title: title };
        tpl = '<li id="{{url}}" class="list-group-item urlli" data-relevance="0">{{title}}<div class="glyphicon glyphicon-remove deleteUrl urldiv redActive hidden" data-toggle="modal" data-target="#myModal"></div><div class="glyphicon glyphicon-refresh repeatUrl urldiv hidden"></div></li>';
        return Mustache.to_html(tpl, data);
    },
    syncUrlList: function syncUrlList (url) {
        var self = $("#" + url);
        self.siblings().removeClass('active');
        self.addClass('active');
        self.siblings().find('.deleteUrl').addClass('hidden');
        var repeatUrlDivs = self.siblings().find('.repeatUrl');
        repeatUrlDivs.addClass('hidden');
        repeatUrlDivs.removeClass('cloroedRep');
        self.find(".deleteUrl").removeClass('hidden');
        self.find(".repeatUrl").removeClass('hidden');
    },
    scrollToUrl: function (url) {
        $('#' + url).ScrollTo();
    },
    scrollToUrlFaster: function (url) {
        $('#' + url).ScrollTo();
    }
};


/*************************/
/*** Sorting functions ***/
/*************************/
var stringRelevanceHelper = {
    get_bigrams: function (string) {
        var i, j, ref, s, v;
        s = string.toLowerCase();
        v = new Array(s.length - 1);
        for (i = j = 0, ref = v.length; j <= ref; i = j += 1) {
            v[i] = s.slice(i, i + 2);
        }
        return v;
    },
    string_similarity: function (str1, str2) {
        var hit_count, j, k, len, len1, pairs1, pairs2, union, x, y;
        if (str1.length > 0 && str2.length > 0) {
            pairs1 = stringRelevanceHelper.get_bigrams(str1);
            pairs2 = stringRelevanceHelper.get_bigrams(str2);
            union = pairs1.length + pairs2.length;
            hit_count = 0;
            for (j = 0, len = pairs1.length; j < len; j++) {
                x = pairs1[j];
                for (k = 0, len1 = pairs2.length; k < len1; k++) {
                    y = pairs2[k];
                    if (x === y) {
                        hit_count++;
                    }
                }
            }
            if (hit_count > 0) {
                return (2.0 * hit_count) / union;
            }
        }
        return 0.0;
    }
};