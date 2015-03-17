$(function(){
	Initialize();
});

function Initialize(){
	var urlVars = getUrlVars();
	var webOverlayId = urlVars["webOverlayId"];
	var callbackUrl = urlVars["callbackUrl"];
	var avatarSession = urlVars["avatarSession"];
	var isObserver = urlVars["isObserver"];
	
	if(isObserver == "True"){
		$('#control-pmm-*').css('visibility', 'hidden');
	}
	
	$.ajaxSetup({ cache: false });
	
	//Refresh timer.
	setInterval(function(){
		$.ajax({
			type: 'GET',
			dataType: 'json',
			crossDomain:true,
			url: callbackUrl + webOverlayId,
			error: function() {
			 $('#main-container').hide();
			},
			success: function (jd) {
			 $('#main-container').show();
			 $('#control-points-value').html(jd.Points);
			 $('#control-totalPoints-value').html(jd.TotalPoints);
			 $('#control-pmm-value').html(jd.PointsPerMove);
			 $('#control-time-value').html(toHHMMSS(jd.Time));
			 $('#control-gameMode-value').html(jd.GameMode);
			}
		});
	}, 500);
	
	$('#control-pmm-add').click(function(){
		$.ajax({
			type: 'PUT',
			dataType: 'json',
			crossDomain:true,
			url: callbackUrl + webOverlayId + '?session=' + avatarSession + '&mode=1',
			success: function (jd) {
				$('#control-pmm-value').html(jd.PointsPerMove);	
			}
		});
	});
	
	$('#control-pmm-subtract').click(function(){
		$.ajax({
			type: 'PUT',
			dataType: 'json',
			crossDomain:true,
			url: callbackUrl + webOverlayId + '?session=' + avatarSession + '&mode=2',
			success: function (jd) {
				$('#control-pmm-value').html(jd.PointsPerMove);	
			}
		});
	});
	
	$('#control-pmm-divide').click(function(){
		$.ajax({
			type: 'PUT',
			dataType: 'json',
			crossDomain:true,
			url: callbackUrl + webOverlayId + '?session=' + avatarSession + '&mode=3',
			success: function (jd) {
				$('#control-pmm-value').html(jd.PointsPerMove);	
			}
		});
	});
	
	$('#control-pmm-multiply').click(function(){
		$.ajax({
			type: 'PUT',
			dataType: 'json',
			crossDomain:true,
			url: callbackUrl + webOverlayId + '?session=' + avatarSession + '&mode=4',
			success: function (jd) {
				$('#control-pmm-value').html(jd.PointsPerMove);	
			}
		});
	});
}

function toHHMMSS(secs) {
	var sec_num = parseInt(secs, 10); // don't forget the second param
	var hours   = Math.floor(sec_num / 3600);
	var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
	var seconds = sec_num - (hours * 3600) - (minutes * 60);

	if (hours   < 10) {hours   = "0"+hours;}
	if (minutes < 10) {minutes = "0"+minutes;}
	if (seconds < 10) {seconds = "0"+seconds;}
	var time    = hours+':'+minutes+':'+seconds;
	return time;
}

function getUrlVars()
{
	var vars = [], hash;
	var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
	for(var i = 0; i < hashes.length; i++)
	{
		hash = hashes[i].split('=');
		vars.push(hash[0]);
		vars[hash[0]] = hash[1];
	}
	return vars;
}