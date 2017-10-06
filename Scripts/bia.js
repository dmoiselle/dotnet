/// <reference path="jquery-1.7.1-vsdoc.js" />

function bia_init_right_hand_links() {
    $(".bia-righthand_links a").button();
}

function bia_candidates() {

    var self = this;

    this.Initialize = function () {
        
        $("#bia-training-site-dropdown").change(function () {
            window.location = "/Training/Candidates?TrainingSiteID=" + $("#bia-training-site-dropdown option:selected").val() + "&TrainingClassID=" + $("#bia-training-class-dropdown option:selected").val() + "&CandidateType=" + $("#bia-candidate-type-dropdown option:selected").val();
        });
        $("#bia-training-class-dropdown").change(function () {
            window.location = "/Training/Candidates?TrainingSiteID=" + $("#bia-training-site-dropdown option:selected").val() + "&TrainingClassID=" + $("#bia-training-class-dropdown option:selected").val() + "&CandidateType=" + $("#bia-candidate-type-dropdown option:selected").val();
        });
        $("#bia-candidate-type-dropdown").change(function () {
            window.location = "/Training/Candidates?TrainingSiteID=" + $("#bia-training-site-dropdown option:selected").val() + "&TrainingClassID=" + $("#bia-training-class-dropdown option:selected").val() + "&CandidateType=" + $("#bia-candidate-type-dropdown option:selected").val();
        });

    }
}

function bia_facilitator_recommendations() {

    var self = this;

    this.Initialize = function () {

        $("#bia-training-site-dropdown").change(function () {
            window.location = "/Training/FacilitatorRecommendations?TrainingSiteID=" + $("#bia-training-site-dropdown option:selected").val() + "&TrainingClassID=" + $("#bia-training-class-dropdown option:selected").val() + "&CandidateType=" + $("#bia-candidate-type-dropdown option:selected").val();
        });
        $("#bia-training-class-dropdown").change(function () {
            window.location = "/Training/FacilitatorRecommendations?TrainingSiteID=" + $("#bia-training-site-dropdown option:selected").val() + "&TrainingClassID=" + $("#bia-training-class-dropdown option:selected").val() + "&CandidateType=" + $("#bia-candidate-type-dropdown option:selected").val();
        });
        $("#bia-candidate-type-dropdown").change(function () {
            window.location = "/Training/FacilitatorRecommendations?TrainingSiteID=" + $("#bia-training-site-dropdown option:selected").val() + "&TrainingClassID=" + $("#bia-training-class-dropdown option:selected").val() + "&CandidateType=" + $("#bia-candidate-type-dropdown option:selected").val();
        });

    }
}

function bia_hiring_candidates() {
    var self = this;
    this.Initialize = function () {
        $("#bia-academy-dropdown").change(function () {
            window.location = "/Hiring/Candidates/?academy=" + $("#bia-academy-dropdown option:selected").val() + "&candidateType=" + $("#bia-candidate-type-dropdown option:selected").val();
        });
        $("#bia-candidate-type-dropdown").change(function () {
            window.location = "/Hiring/Candidates/?academy=" + $("#bia-academy-dropdown option:selected").val() + "&candidateType=" + $("#bia-candidate-type-dropdown option:selected").val();
        });
    }
}

function bia_wage_grades() {
    var self = this;
    this.Initialize = function () {
        $("#bia-hiring-candidate-type-dropdown").change(function () {
            window.location = "/Administration/WageGrades/?candidateType=" + $("#bia-hiring-candidate-type-dropdown option:selected").val() + "&wageCategoryID=" + $("#bia-wage-category-dropdown option:selected").val();
        });

        $("#bia-wage-category-dropdown").change(function () {
            window.location = "/Administration/WageGrades/?candidateType=" + $("#bia-hiring-candidate-type-dropdown option:selected").val() + "&wageCategoryID=" + $("#bia-wage-category-dropdown option:selected").val();
        });
    }
}

function bia_construct_candidate_filter() {
    var academy = $("#bia-target-academy-dropdown option:selected").val();
    var candidateType = $("#bia-candidate-type-dropdown option:selected").val();
    var starRating = $("#bia-star-rating-dropdown option:selected").val();
    var hiringStatus = $("#bia-hiring-status-dropdown option:selected").val();

    var params = "";

    if (academy != "") {
        if (params == "") {
            params = "targetAcademy=" + academy;
        }
        else {
            params = params + "&targetAcademy=" + academy;
        }
    }
    if (candidateType != "") {
        if (params == "") {
            params = "candidateType=" + candidateType;
        }
        else {
            params = params + "&candidateType=" + candidateType; ;
        }
    }
    if (hiringStatus != "") {
        if (params == "") {
            params = "hiringStatus=" + hiringStatus;
        }
        else {
            params = params + "&hiringStatus=" + hiringStatus;
        }
    }
    if (starRating != "") {
        if (params == "") {
            params = "starRating=" + starRating;
        }
        else {
            params = params + "&starRating=" + starRating;
        }
    }
    return params;
}

function bia_hiring_new_candidates() {
    var self = this;
    this.Initialize = function () {
        $("#bia-star-rating-dropdown").change(function () {
            var filters = bia_construct_candidate_filter();
            window.location = "/Hiring/Candidates/?" + filters;
        });

        $("#bia-target-academy-dropdown").change(function () {
            var filters = bia_construct_candidate_filter(); 
            window.location = "/Hiring/Candidates/?" + filters;
        });

        $("#bia-candidate-type-dropdown").change(function () {
            var filters = bia_construct_candidate_filter(); 
            window.location = "/Hiring/Candidates/?" + filters;
        });

        $("#bia-hiring-status-dropdown").change(function () {
            var filters = bia_construct_candidate_filter();
            window.location = "/Hiring/Candidates/?" + filters;
        });
    }
}

function bia_positions() {

    var self = this;
    this.Initialize = function () {

        $("#bia-position-status-dropdown").change(function () {
            bia_positions_refresh();
        });

        $("#bia-position-academy-dropdown").change(function () {
            bia_positions_refresh();
        });

        $("#bia-position-candidate-type-dropdown").change(function () {
            bia_positions_refresh();
        });
    }
}


function bia_positions_refresh() {

        var academy = $("#bia-position-academy-dropdown option:selected").val();
        var positionStatus = $("#bia-position-status-dropdown option:selected").val();
        var candidateType = $("#bia-position-candidate-type-dropdown option:selected").val();

        window.location = "/Hiring/Positions/?" + bia_construct_position_filter(academy, positionStatus, candidateType);
}

function bia_construct_position_filter(academy, positionStatus, candidateType) {

    var params = "";

    if (positionStatus != "") {

        if (params == "") {
            params = "positionStatus=" + positionStatus;
        }
        else {
            params = params + "&positionStatus=" + positionStatus; ;
        }

    }

    if (candidateType != "") {

        if (params == "") {
            params = "candidateType=" + candidateType;
        }
        else {
            params = params + "&candidateType=" + candidateType; ;
        }

    }

    if (academy != "") {

        if (params == "") {
            params = "academy=" + academy;
        }
        else {
            params = params + "&academy=" + academy;
        }

    }

    return params;
}

function bia_show_dialog(title, message) {
    $("#bia-message-dialog:ui-dialog").dialog("destroy"); 
    $("#bia-message-dialog-content").html(message);

    $("#bia-message-dialog").dialog({
        title: title,
        resizable: "false",
        modal: true,
        width: "420",
        buttons: {
            OK: function () {
                $("#bia-message-dialog-content").html("");
                $(this).dialog("close");
            }
        }
    });
}

function bia_show_confirmation_dialog(title, message, onOK, onCancel) {
    $("#bia-message-dialog:ui-dialog").dialog("destroy");
    $("#bia-message-dialog-content").html(message);

    var btns = {};
    // plug the OK button only if an onOK callback was supplied
    if ($.isFunction(onOK)) {
        btns["OK"] = function () {            
            onOK();
            $("#bia-message-dialog-content").html("");
            $(this).dialog("close");
        }
    }
    // plug the Cancel button irrespective, but only fire the onCancel callback if it was supplied
    btns["Cancel"] = function () {
                if ($.isFunction(onCancel)) {
                    onCancel();
                }
                $("#bia-message-dialog-content").html("");
                $(this).dialog("close");
            };


    $("#bia-message-dialog").dialog({
        title: title,
        resizable: "false",
        modal: true,
        width: "420",
        buttons: btns
    });
}

function bia_relocate_candidates() {
    var self = this;
    this.Initialize = function () {


        $('#bia-candidate-type').val($("#bia-candidate-type-dropdown option:selected").val());

        $("#bia-academy-dropdown").change(function () {
            $('#bia-academy').val($("#bia-academy-dropdown option:selected").val());
            $('#bia-candidate-type').val($("#bia-candidate-type-dropdown option:selected").val());
            window.location = "/Hiring/RelocateCandidates/?academy=" + $("#bia-academy-dropdown option:selected").val() + "&candidateType=" + $("#bia-candidate-type-dropdown option:selected").val();

        });

        $("#bia-candidate-type-dropdown").change(function () {
            $('#bia-academy').val($("#bia-academy-dropdown option:selected").val());
            $('#bia-candidate-type').val($("#bia-candidate-type-dropdown option:selected").val());
            window.location = "/Hiring/RelocateCandidates/?academy=" + $("#bia-academy-dropdown option:selected").val() + "&candidateType=" + $("#bia-candidate-type-dropdown option:selected").val();

        });

    }
}

function bia_candidates_contract_status() {
    var self = this;
    this.Initialize = function () {
    
        $("#bia-academy-dropdown").change(function () {
            
            var academy = $("#bia-academy-dropdown option:selected").val() == "" ? 'All' : $("#bia-academy-dropdown option:selected").val();

            var candidateType = $("#bia-candidate-type-dropdown option:selected").val() == "" ? 'All' : $("#bia-candidate-type-dropdown option:selected").val();
            
            window.location = "/Hiring/ContractStatuses/?academy=" + academy + "&candidateType=" + candidateType;
        });

        $("#bia-candidate-type-dropdown").change(function () {
            var academy = $("#bia-academy-dropdown option:selected").val() == "" ? 'All' : $("#bia-academy-dropdown option:selected").val();

            var candidateType = $("#bia-candidate-type-dropdown option:selected").val() == "" ? 'All' : $("#bia-candidate-type-dropdown option:selected").val();

            window.location = "/Hiring/ContractStatuses/?academy=" + academy + "&candidateType=" + candidateType;
        });        

    }
}

function bia_candidates_contract_types() {
    var self = this;
    this.Initialize = function () {

        $("#bia-hiring-candidate-type-dropdown").change(function () {

            var candidateType = $("#bia-hiring-candidate-type-dropdown option:selected").val() == "" ? 'All' : $("#bia-hiring-candidate-type-dropdown option:selected").val();

            var contractType = $("#bia-contract-type-dropdown option:selected").val() == "" ? 'All' : $("#bia-contract-type-dropdown option:selected").val();

            window.location = "/Hiring/Contracts?contractType=" + contractType + "&candidateType=" + candidateType;
        });

        $("#bia-contract-type-dropdown").change(function () {
            var candidateType = $("#bia-hiring-candidate-type-dropdown option:selected").val() == "" ? 'All' : $("#bia-hiring-candidate-type-dropdown option:selected").val();

            var contractType = $("#bia-contract-type-dropdown option:selected").val() == "" ? 'All' : $("#bia-contract-type-dropdown option:selected").val();

            window.location = "/Hiring/Contracts?contractType=" + contractType + "&candidateType=" + candidateType;
        });

    }
}


function bia_candidates_letters() {
    var self = this;
    this.Initialize = function () {

        $("#bia-hiring-candidate-type-dropdown").change(function () {

            var candidateType = $("#bia-hiring-candidate-type-dropdown option:selected").val() == "" ? 'All' : $("#bia-hiring-candidate-type-dropdown option:selected").val();

            window.location = "/Hiring/Letters?candidateType=" + candidateType;
        });

        
    }
}

function bia_candidate_scores() {

    var self = this;

    this.Initialize = function () {
        $("#bia-candidate-status-dropdown").change(function () {
            self.postAction();
        });

    }

    this.postAction = function () {
        $.post("/Training/CandidateStatusInsert/", $("#bia-candidate-scores-form").serialize(), function () {
            alert("Insert successful");           

        });

    }

}

function bia_assessmentsGUI() {
    var self = this;
    var _recruitmentCycleScoringRules;
    var _trainingClassSelect = $("#TrainingClass");
    var _candidateTypeSelect = $("#CandidateType");
    var _weekNumberSelect = $("#WeekNumberSelect");
    var _combinedAssessmentTypeSelect = $("#AssessmentTypeSelect");
    var _weekNumberInput = $("#WeekNumber");
    var _assessmentTypeInput = $("#AssessmentType");
    var _typeNumberInput = $("#TypeNumber");
    var _url = $("#AssessmentsURL");
    var _nationalIDInput = $("#NationalID");
    var _scoreInput = $("#Score");
    var _submitScore = $("#submitScore");

    this.Initialize = function () {
        self.initializeValidation();
        self.initializeAssessmentOptionDropdowns();
        self.initializeTable();
    }

    this.initializeValidation = function () {
        var assessmentValidator = $("#bia-assessment-entry-form").validate({
            rules: {
                CandidateType: { required: true },
                WeekNumber: { required: true },
                CombinedAssessmentType: { required: true },
                NationalID: { required: true },
                Score: {
                    required: true,
                    min: 0,
                    max: 35
                }
            },
            messages: {
                CandidateType: { required: "A Candidate Type must be selected" },
                WeekNumber: { required: "A Week Number must be selected" },
                CombinedAssessmentType: { required: "An Assessment Type must be selected" },
                NationalID: { required: "National ID is required" },
                Score: {
                    max: "Score must be between 0 and 35",
                    min: "Score must be between 0 and 35",
                    required: "Score is required"
                }
            }
        });

    }

    this.initializeAssessmentOptionDropdowns = function () {
        _recruitmentCycleScoringRules = $.parseJSON($("#RecruitmentCycleScoringRules").val());

        var candidateType = _candidateTypeSelect.find("option:selected").val();

        self.setWeekNumberSelector(candidateType);
        self.setAssessmentTypeSelector(candidateType, _weekNumberInput.val());

        _trainingClassSelect.change(function () {
            _candidateTypeSelect.val("&lt;Select Item&gt");
            self.setWeekNumberSelector();
            self.setAssessmentTypeSelector();
            self.clearTable();
        });

        _candidateTypeSelect.change(function () {
            _assessmentTypeInput.val("");
            _weekNumberInput.val("");
            self.setWeekNumberSelector($(this).find("option:selected").val());
            self.setAssessmentTypeSelector();

        });

        _weekNumberSelect.change(function () {
            var selectedWeekNumber = $(this).find("option:selected").val();

            _weekNumberInput.val(selectedWeekNumber);
            _assessmentTypeInput.val("");

            self.setAssessmentTypeSelector(
                _candidateTypeSelect.find("option:selected").val(),
                selectedWeekNumber);

        });

        _combinedAssessmentTypeSelect.live('change', function () {
            var selectedOption = $(this).find("option:selected");
            var splitOptions = selectedOption.val().split(",");

            if (splitOptions.length > 1) {
                _assessmentTypeInput.val(splitOptions[0]);
                _typeNumberInput.val(splitOptions[1]);
            }
            else {
                _assessmentTypeInput.val(splitOptions[0]);
                _typeNumberInput.val("");
            }
            _url = "/Training/PartialAssessments/?trainingClass=" + _trainingClassSelect.val() + "&candidateType=" + _candidateTypeSelect.val() + "&typeNumber=" + _typeNumberInput.val() + "&weekNumber=" + _weekNumberInput.val() + "&assessmentType=" + _assessmentTypeInput.val();
            $.get(_url, function (data) {
                $('#assessments-listing').replaceWith(data);
                self.initializeTable();
            });

        });
    }

    this.setWeekNumberSelector = function (candidateType) {

        var weekSelectOptions = [];
        weekSelectOptions.push("<option value=''>&lt;Select Item&gt</option>");

        var candidateTypeScoringRules;
        if (candidateType != undefined && candidateType != "") {
            candidateTypeScoringRules = self.getCandidateTypeScoringRules(candidateType);
            for (var weekNumberIndex = 0; weekNumberIndex < candidateTypeScoringRules.Weeks.length; weekNumberIndex++) {
                var weeklyRuleSet = candidateTypeScoringRules.Weeks[weekNumberIndex];
                var selected = "";
                if (weeklyRuleSet.WeekNumber == _weekNumberInput.val()) {
                    selected = " selected";
                }
                weekSelectOptions.push("<option value='" + weeklyRuleSet.WeekNumber + "'" + selected + ">Week " + weeklyRuleSet.WeekNumber + "</option>");
            }
        }

        _weekNumberSelect.html(weekSelectOptions.join(''));
    }

    this.setAssessmentTypeSelector = function (candidateType) {
        var assessmentTypeOptions = [];
        assessmentTypeOptions.push("<option value=''>&lt;Select Item&gt</option>");

        var weekNumber = _weekNumberSelect.find("option:selected").val();
        if (candidateType != undefined && candidateType != "" && weekNumber != undefined && weekNumber != "") {
            var weekScoringRuleSet = self.getWeeklyScoringRules(candidateType, weekNumber);
            for (var ruleIndex = 0; ruleIndex < weekScoringRuleSet.Rules.length; ruleIndex++) {
                var rule = weekScoringRuleSet.Rules[ruleIndex];
                var selected = "";
                if (rule.TypeNumber != undefined && rule.TypeNumber != "") {
                    if (rule.TypeNumber == _typeNumberInput.val() && rule.Type == _assessmentTypeInput.val()) {
                        selected = " selected";
                    }
                    assessmentTypeOptions.push("<option value='" + rule.Type + "," + rule.TypeNumber + "'" + selected + ">" + rule.Type + " #" + rule.TypeNumber + "</option>");
                }
                else {
                    if (rule.Type == _assessmentTypeInput.val()) {
                        selected = " selected";
                    }
                    assessmentTypeOptions.push("<option value='" + rule.Type + "'" + selected + ">" + rule.Type + "</option>");
                }
            }


        }

        _combinedAssessmentTypeSelect.html(assessmentTypeOptions.join(''));
    }

    this.getCandidateTypeScoringRules = function (candidateType) {
        for (var i = 0; i < _recruitmentCycleScoringRules.CandidateTypeRules.length; i++) {
            if (_recruitmentCycleScoringRules.CandidateTypeRules[i].CandidateType == candidateType) {
                return _recruitmentCycleScoringRules.CandidateTypeRules[i];
            }
        }
    }

    this.getWeeklyScoringRules = function (candidateType, weekNumber) {
        var candidateRules = self.getCandidateTypeScoringRules(candidateType);
        for (var i = 0; i < candidateRules.Weeks.length; i++) {
            if (candidateRules.Weeks[i].WeekNumber == weekNumber) {
                return candidateRules.Weeks[i];
            }
        }
    }

    this.initializeTable = function () {
        var _assessmentTable = $("#bia-assessment-table").dataTable({
            "bProcessing": true,
            bJQueryUI: true,
            sPaginationType: "full_numbers",
            bDestroy: true,
            fnDrawCallBack: bindColumn()
        });

        function updateScore(inputElem, parent, candidateid, assessmentid, score) {
            var value = inputElem.val();

            $.post("Assessment/?CandidateType=" + _candidateTypeSelect.val() + "&AssessmentType=" + _assessmentTypeInput.val() + "&TypeNumber=" + _typeNumberInput.val() + "&WeekNumber=" + _weekNumberInput.val() + "&AssessmentID=" + assessmentid + "&RawTestScore=" + value + "&CandidateID=" + candidateid, function (result) {
                if (result.success == true) {
                    parent.attr({ "assessmentid": result.AssessmentID });
                    parent.attr({ "score": value });
                    parent.text(value);
                }
                else {
                    bia_show_dialog("Error", result.error);
                    if (score == "") {
                        parent.text("Double-click to edit");
                    }
                    else {
                        parent.text(score);
                    }
                }
            });
        }

        function bindColumn() {

            $(".bia-assessment-score").unbind('dblclick').bind('dblclick', function () {

                var target = $(this);
                var candidateid = target.attr("candidateid");
                var assessmentid = target.attr("assessmentid");
                var score = target.attr("score");
                var elementID = candidateid + "" + assessmentid;

                target.html("<input type='input' id='" + elementID + "' value='" + score + "'/>");

                $("#" + elementID).focus();

                $("#" + elementID).bind("keyup", function (e) {
                    if (e.which == 13) {
                        updateScore($(this), target, candidateid, assessmentid, score);
                    }
                }).bind("focusout", function () {
                    updateScore($(this), target, candidateid, assessmentid, score);
                });
            });
        }
    }

    this.clearTable = function () {
        $("#bia-assessment-table tbody").replaceWith('<tbody>/n</tbody>');
        self.initializeTable();
    }

    this.postAction = function () {
        $.post("/Training/Assessment/", $("#bia-assessment-entry-form").serialize(), function (data) {
            if ($(data).text().search(new RegExp(/ERROR/)) > 0) {
                $('#Error').empty().prepend(data);
            }
            else {
                $('#bia-assessments-table tbody').prepend(data);
                _nationalIDInput.val("");
                _scoreInput.val("");
                $('#Error').empty()
            }

        });        
    }

}

(function ($) {
    $.fn.emptySelect = function () {
        return this.each(function () {
            if (this.tagName == 'SELECT') this.options.length = 0;
        });
    }

    $.fn.loadSelect = function (optionsDataArray) {
        return this.emptySelect().each(function () {
            if (this.tagName == 'SELECT') {
                var selectElement = this;
                $.each(optionsDataArray, function (index, optionData) {
                    var option = new Option(optionData.caption,
                                  optionData.value);
                    if ($.browser.msie) {
                        selectElement.add(option);
                    }
                    else {
                        selectElement.add(option, null);
                    }
                });
            }
        });
    }
})(jQuery);