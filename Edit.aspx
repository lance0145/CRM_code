<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="CRM.Admin.Pools.Edit" MasterPageFile="~/Layout.Master" %>

<asp:Content ID="Content3" ContentPlaceHolderID="PostHeadContent" runat="server" ClientIDMode="Static">
    <style>
     
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="LayoutMainContent" runat="server" ClientIDMode="Static">
    <div class="content-wrapper" id="pool-default-agent">

        <!--Start of Page Title-->
        <section class="content-header">
            <div class="page-title-cont">
                <i class="page-title-i">
                    <span class="page-title-icon"><i class="fa fa-list-alt"></i></span>
                    <span class="page-title-text">Edit Pool</span>
                </i>
            </div>

            <ol class="breadcrumb">
                <li><a href="#"><i class="fa fa-home"></i>Home</a></li>
                <li><a href="#">Admin</a></li>
                <li><a href="#">Pools</a></li>
                <li class="active">Edit Pool</li>
            </ol>

            <hr class="hr-new" />
        </section>
        <!--End of Page Title-->

        <section class="content">
            <div class="row">
                <div class="col-sm-12 col-md-8 col-lg-8">

                    <div class="">
                        <a href="/Admin/Pools/Index.aspx" class="btn btn-sm btn-primary">
                            <span class="fa fa-arrow-left"></span>Back to Pool List
                        </a>
                    </div>

                    <br />

                    <div class="box box-info">
                        <div class="box-header with-border">
                            <h3 class="box-title">Edit Pool</h3>
                        </div>

                        <div class="box-body">

                            <form id="createForm" method="post">

                                <input type="hidden" name="P_Id" value="<%= EPool.id %>" />

                                <div class="row">
                                    <div class="col-sm-12 col-md-12 col-lg-12">
                                        <div class="form-group">
                                            <label for="P_Name">Name <i class="text-danger">*</i></label>
                                            <input type="text" name="P_Name" id="P_Name" class="form-control" value="<%= SessionPop("P_Name") ?? EPool.name ?? "" %>" required />
                                        </div>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-12 col-md-12 col-lg-12">
                                        <div class="form-group">
                                            <label for="P_MT_Login_Start">MT Login Start <i class="text-danger">*</i></label>
                                            <input type="number" name="P_MT_Login_Start" id="P_MT_Login_Start" value="<%=  EPool.MT_Login_Start  %>" class="form-control" required />
                                        </div>
                                        <div class="form-group">
                                            <label for="P_MT_Login_End">MT Login End <i class="text-danger">*</i></label>
                                            <input type="number" name="P_MT_Login_End" id="P_MT_Login_End" value="<%=  EPool.MT_Login_End  %>" class="form-control" required />
                                        </div>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-md-6">
                                        <label for="P_MT_Login_Start">Balance for withdrawal requestsl <span data-toggle="tooltip" data-placement="top" title="Balance amount that allows unverified profiles to make withdrawals."><i class="fa fa-question-circle"></i></span><i class="text-danger">*</i></label>
                                        <input type="number" name="maxWithdrawal" id="maxWithdrawal" value="<%=EPool.max_withdrawal %>" class="form-control" required />
                                    </div>
                                    <div class="form-group col-md-3">
                                        <label>Enable?</label><br />
                                        <%--<input type="checkbox" id="enableMax" name="enableMax" value="<%= EPool.is_enable %>" class="form-group" />--%>
                                        <input type="checkbox" id="chkEnableMax" runat="server" name="chkEnableMax" clientidmode="Static" />
                                    </div>
                                    <div class="form-group col-md-3">
                                        <label>Send MT Daily P/N SMS Report to Client?</label><br />
                                        <input type="checkbox" id="chkSendReport" runat="server" name="chkSendReport" clientidmode="Static" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-12 col-md-12 col-lg-12">
                                        <div class="form-group">
                                            <label for="P_AutoRet">Auto Convert to retention?<i class="text-danger">*</i></label>
                                            <select id="P_AutoRet" name="P_AutoRet" class="form-control selectpicker" data-live-search="true">
                                                <%Response.Write(AutoConvertOptions); %>
                                            </select>
                                        </div>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-12 col-md-12 col-lg-12">
                                        <div class="form-group">
                                            <label for="P_AutoContractsClose">Auto Close Contracts on expiration?<i class="text-danger">*</i></label>
                                            <select id="P_AutoContractsClose" name="P_AutoContractsClose" class="form-control selectpicker" data-live-search="true">
                                                <%Response.Write(AutoContractsCloseOptions); %>
                                            </select>
                                        </div>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-12 col-md-6 col-lg-6">
                                        <div class="form-group">
                                            <label for="P_Countries">Countries</label>
                                            <select id="P_Countries" name="P_Countries" class="form-control selectpicker" data-live-search="true" multiple>
                                                <optgroup id="countryOptions" runat="server" label=""></optgroup>
                                            </select>
                                        </div>
                                    </div>
                                    <div class="col-sm-12 col-md-6 col-lg-6">
                                        <div class="form-group">
                                            <label for="P_MTG">MT Groups <i class="text-danger">*</i></label>
                                            <select name="P_MTG" id="P_MTG" class="form-control selectpicker" multiple data-live-search="true" required>
                                                <optgroup id="mtgOptions" runat="server" label=""></optgroup>
                                            </select>
                                        </div>
                                    </div>
                                </div>

                                <div class="row">
                                    <div class="col-sm-12 col-md-12 col-lg-12">
                                        <div class="form-group">
                                            <label>Referrals</label>

                                            <p id="refItemIndicator" class="text-center text-danger">No referral added.</p>
                                            <p id="alreadyExist" class="text-center text-danger" style="display: none"><i>Referral ID is Already Existing!</i></p>
                                            <p id="available" class="text-center text-success" style="display: none"><i>Referral ID is available</i></p>

                                            <div class="reflist"></div>
                                            <a href="#!" class="pull-right" id="add-ref"><u>Add referral</u></a>
                                            <br style="clear: both;" />
                                        </div>
                                    </div>
                                </div>
                                                                
                                <div class="row">
                                    <div class="col-sm-12 col-md-1 col-lg-12">
                                        <div class="form-group">
                                            <button type="submit" id="update" class="btn btn-primary pull-right">Update <i class="fa fa-arrow-circle-right"></i></button>
                                        </div>
                                    </div>
                                </div>
                                <br/>
                                <br/>
                                <br/>

                                <default-user-per-status-widget poolid="<%= EPool.id %>"></default-user-per-status-widget>

                            </form>

                        </div>

                    </div>

                </div>
            </div>
        </section>

    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="PostBodyContent" runat="server" ClientIDMode="Static">

    <script src="<%=BrandsMasterCdn %>script/dynamic-search-scripts/dist/poolDefaultAgent.js"></script>
    <script type="text/javascript">

        var strPools = "<%Response.Write(strPools);%>";
        <%if (strPools != null)
        { %>
            $("#P_Countries").selectpicker('val', strPools.split('!#!'));
        <%}%>
        console.log(strPools.split(','))

        function CreateTemplateElelement(value = "") {

            let inputGroup = document.createElement('div');
            $(inputGroup).addClass('input-group ref-item');

            let inputGroupAddOn = document.createElement('span');
            $(inputGroupAddOn).addClass('input-group-addon');
            $(inputGroupAddOn).text('Referrer ID');

          let input = document.createElement('input');
            $(input).attr('type', 'text');
            $(input).attr('name', 'P_Ref');
            $(input).attr('onChange', 'checkIfExist(this.value)');

            //$(input).attr('pattern', '\\d*');
            //$(input).attr('placeholder', 'numeric characters only');

            if (value.length > 0) {
                $(input).val(value);
            }

            $(input).addClass('form-control');

            let inputGroupAddOnTail = document.createElement('span');
            $(inputGroupAddOnTail).addClass('input-group-btn');

            let checkBtn = document.createElement('button');
            $(checkBtn).addClass('btn btn-success');
            $(checkBtn).attr('type', 'button');
            $(checkBtn).text('Check');
            $(inputGroupAddOnTail).append(checkBtn);

            let removeBtn = document.createElement('a');
            $(removeBtn).addClass('remove-ref-item btn btn-danger');
            $(removeBtn).attr('href', '#!');
            $(removeBtn).text('remove');
            $(inputGroupAddOnTail).append(removeBtn);


            $(inputGroup).append(inputGroupAddOn);
            $(inputGroup).append(input);
            $(inputGroup).append(inputGroupAddOnTail);

            $(inputGroup).css('margin-bottom', '5px');

            return inputGroup;

        }

        function RefItemIndicator() {

            if ($('.ref-item').length <= 0) {
                $('#refItemIndicator').removeClass('hidden');
                return;
            }

            $('#refItemIndicator').addClass('hidden');
        }


        function PopulateSelected(selected) {
            $(selected).each(function (i, e) {
                $('.reflist').append(CreateTemplateElelement(e));
            });
        }


        $(document).ready(function () {



            var selectedData = <%= SessionPop("P_Ref") ?? SessionPop("Old_P_Ref") ?? "[]"%>;

            PopulateSelected(selectedData);

            RefItemIndicator();

            $('#add-ref').click(function (e) {
                e.preventDefault();
                $('.reflist').append(CreateTemplateElelement());

                $(".reflist").scrollTop($(".reflist")[0].scrollHeight);

                RefItemIndicator();
            });

            $('body').on('click', '.remove-ref-item', function () {

                let res = confirm("Are you sure you want to delete this field?");

                if (res)
                    $(this).parents('.ref-item').remove();
                    $('#update').removeAttr('disabled');
                    $("#add-ref").show();

                RefItemIndicator();
            });

        });

        function checkIfExist(val) {
            if (val) {
                $.ajax({
                    type: 'POST',
                    url: '/Admin/Pools/Create.aspx/CheckReferralExist',
                    contentType: 'application/json; charset = utf-8',
                    data: "{'referral':'" + val + "'}",
                    success: function (data) {
                        var obj = data.d;
                        if (obj != "False") {
                            $("#alreadyExist").show();
                            $('#update').attr('disabled', 'disabled');
                            $('#add-ref').hide();
                            setTimeout(function () {
                                $("#alreadyExist").hide();
                            }, 3000);
                        } else {
                            $('#update').removeAttr('disabled');
                            $("#available").show();
                            $("#add-ref").show();
                            setTimeout(function () {
                                 $("#available").hide();
                            }, 3000);
                        }
                    },
                    error: function (result) {
                        console.log("error");
                    }
                });
            }
        }


        <% var t = GetToast(); %>

        <%if (t != null)
        {%>
        toastr.<%=t.Value.Key%>('<%=t.Value.Value%>');
        <%}%>


</script>

</asp:Content>
