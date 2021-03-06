using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation;
using System.Collections;
using iControlSnapIn.TypeData;

namespace iControlSnapIn.CmdLet.LTM.PoolMember
{
    [Cmdlet(VerbsLifecycle.Enable, iControlNouns.LTMPoolMember, SupportsShouldProcess = true)]
    public class EnableLTMPoolMember : iControlPSCmdlet
    {

        #region Parameters

        private string _pool = null;
        [Parameter(Position=0, HelpMessage="The name of the LTM Pool")]
        [ValidateNotNullOrEmpty]
        public string Pool
        {
            get { return _pool; }
            set { _pool = value; }
        }

        private string _member = null;
        [Parameter(Position=1, HelpMessage="The IP:port of the LTM Pool Member")]
        [ValidateNotNullOrEmpty]
        public string Member
        {
            get { return _member; }
            set { _member = value; }
        }

		private LTMPoolMemberItem _pool_member_item;
		[Parameter(Position = 2, HelpMessage = "The LTMPoolMemberItem describing the pool member", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
		[Alias("item")]
		[ValidateNotNullOrEmpty]
		public LTMPoolMemberItem PoolMember
		{
			get { return _pool_member_item; }
			set { _pool_member_item = value; }
		}

		#endregion

		private bool ValidateInput()
		{
			bool bValid = false;
			if ((null != _pool) && (null != _member))
			{
				bValid = true;
			}
			else if (null != _pool_member_item)
			{
				if ((null != _pool_member_item.Address) &&
					 (null != _pool_member_item.Pool))
				{
					if ((0 != _pool_member_item.Pool.Length) ||
						 (0 != _pool_member_item.Address.Length))
					{
						Pool = _pool_member_item.Pool;
						Member = _pool_member_item.Address + ":" + _pool_member_item.Port.ToString();
						bValid = true;
					}
				}
			}
			if (!bValid)
			{
				handleError("You must specify either the Pool and Member, or the PoolMember parameter ", "Input Validation Error");
			}
			return bValid;
		}

		protected override void ProcessRecord()
        {
			if (ValidateInput())
			{
				if (isInitialized())
				{
					try
					{
						if (null == _pool)
						{
							handleError("You must supply a pool name", "bad state");
						}
						else if (null == _member)
						{
							handleError("You must supply a member ip:port definition", "bad state");
						}
						else
						{
							String[] sSplit = _member.Split(new char[] { ':' });
							if (2 == sSplit.Length)
							{
								String[] pool_list = new String[] { _pool };
								iControl.LocalLBPoolMemberMemberSessionState[][] session_states = new iControl.LocalLBPoolMemberMemberSessionState[1][];
								session_states[0] = new iControl.LocalLBPoolMemberMemberSessionState[1];
								session_states[0][0] = new iControl.LocalLBPoolMemberMemberSessionState();
								session_states[0][0].member = new iControl.CommonIPPortDefinition();
								session_states[0][0].member.address = sSplit[0];
								session_states[0][0].member.port = Convert.ToInt32(sSplit[1]);
								session_states[0][0].session_state = iControl.CommonEnabledState.STATE_ENABLED;
								GetiControl().LocalLBPoolMember.set_session_enabled_state(pool_list, session_states);

								LTMPoolMemberItem pmi = new LTMPoolMemberItem();
								pmi.Pool = Pool;
								pmi.Address = sSplit[0];
								pmi.Port = Convert.ToInt32(sSplit[1]);
								pmi.Name = sSplit[0] + ":" + Convert.ToInt32(sSplit[1]);
								pmi.Availability = iControl.LocalLBAvailabilityStatus.AVAILABILITY_STATUS_NONE;
								pmi.Enabled = iControl.LocalLBEnabledStatus.ENABLED_STATUS_ENABLED;
								pmi.Status = "";

								WriteObject(pmi);
							}
							else
							{
								handleError("Invalid member ip:port format", "bad state");
							}
						}
					}
					catch (Exception ex)
					{
						ErrorRecord er = new ErrorRecord(ex, "2", ErrorCategory.OpenError, "error");
						WriteError(er);
					}
				}
				else
				{
					handleError("You must first successfully call Initialize-iControl", "bad state");
				}
			}
        }
    }
}
