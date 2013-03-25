/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

package multiverse.mars.core;

import multiverse.server.util.*;
import java.util.*;
import java.util.concurrent.locks.*;

/**
 * the trade session stores the state and objects being traded
 */
public class TradeSession {
    /**
     * the pair you pass in is going to set who is trader1
     * and trader2 in the trading list.
     */
    public TradeSession(Long trader1, Long trader2) {
	this.trader1 = trader1;
	this.trader2 = trader2;
    }

    /**
     * the trader is attempting to set the offer for one trader
     * returns true if it succeeds
     */
    public boolean setOffer(Long trader, List<Long> offer) {
	sessionLock.lock();
	try {
	    if (trader.equals(trader1)) {
		offer1 = offer;
	    }
	    else if (trader.equals(trader2)) {
		offer2 = offer;
	    }
	    else {
		return false;
	    }
	    return true;
	}
	finally {
	    sessionLock.unlock();
	}
    }

    /**
     * update offer for trader.
     * possibly reset accepted flag for other trader if offer changed
     */
    public boolean updateOffer(Long trader, List<Long> offer, boolean accepted) {
	sessionLock.lock();
	try {
	    if (!isTrader(trader)) {
		return false;
	    }
	    List<Long> oldOffer = getOffer(trader);
	    if (!oldOffer.equals(offer)) {
		setAccepted(getPartnerOid(trader), false);
	    }
	    setOffer(trader, offer);
	    setAccepted(trader, accepted);
	    return true;
	}
	finally {
	    sessionLock.unlock();
	}
    }

    public Long getTrader1() { return trader1; }
    public Long getTrader2() { return trader2; }

    public boolean isTrader(Long trader) {
	if (trader.equals(trader1) || trader.equals(trader2)) {
	    return true;
	}
	else {
	    return false;
	}
    }

    public Long getPartnerOid(Long trader) {
	Log.debug("TradeSession.getPartnerOid: trader=" + trader
		  + " trader1=" + trader1 + " trader2=" + trader2);
	if (trader.equals(trader1)) {
	    return trader2;
	}
	else if (trader.equals(trader2)) {
	    return trader1;
	}
	else {
	    Log.error("TradeSession.getPartnerOid: trader=" + trader + " not party to this session=" + this);
	    throw new MVRuntimeException("invalid trader");
	}
    }

    public List<Long> getOffer(Long trader) {
	sessionLock.lock();
	try {
	    if (trader.equals(trader1)) {
		return offer1;
	    }
	    else if (trader.equals(trader2)) {
		return offer2;
	    }
	    else {
		Log.error("TradeSession.getOffer: trader=" + trader + " not party to this session=" + this);
		throw new MVRuntimeException("invalid trader");
	    }
	}
	finally {
	    sessionLock.unlock();
	}
    }

    public boolean getAccepted(Long trader) {
	sessionLock.lock();
	try {
	    if (trader.equals(trader1)) {
		return accepted1;
	    }
	    else if (trader.equals(trader2)) {
		return accepted2;
	    }
	    else {
		Log.error("TradeSession.getAccepted: trader=" + trader + " not party to this session=" + this);
		throw new MVRuntimeException("invalid trader");
	    }
	}
	finally {
	    sessionLock.unlock();
	}
    }

    public void setAccepted(Long trader, boolean val) {
	sessionLock.lock();
	try {
	    if (trader.equals(trader1)) {
		accepted1 = val;
	    }
	    else if (trader.equals(trader2)) {
		accepted2 = val;
	    }
	    else {
		Log.error("TradeSession.setAccepted: trader=" + trader + " not party to this session=" + this);
		throw new MVRuntimeException("invalid trader");
	    }
	}
	finally {
	    sessionLock.unlock();
	}
    }

    public boolean isComplete() {
	sessionLock.lock();
	try {
	    return (accepted1 && accepted2);
	}
	finally {
	    sessionLock.unlock();
	}
    }

    private Long trader1 = null;
    private Long trader2 = null;

    /**
     * list of objects trader1 is giving
     */
    private List<Long> offer1 = new LinkedList<Long>();

    /**
     * list of objects trader2 is giving
     */
    private List<Long> offer2 = new LinkedList<Long>();

    private boolean accepted1 = false;
    private boolean accepted2 = false;

    /**
     * sometimes handlers need the lock - eg, they check for the
     * state, do something, then set the new state
     */
    public Lock getLock() {
	return sessionLock;
    }
    transient private Lock sessionLock = LockFactory.makeLock("TradeSessionLock");
}
